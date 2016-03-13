using System;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FiberKartan.Database;
using FiberKartan.Database.Models;
using log4net;

namespace FiberKartan.API.Security
{
    public sealed class SecurityHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Genererar ett unikt hashvärde utifrån användarnamnet och lösenordet.
        /// </summary>
        /// <param name="username">Användarnamn</param>
        /// <param name="password">Lösenord</param>
        /// <returns>Unikt hashvärde</returns>
        private static string GeneratePasswordHash(string username, string password)
        {
            return Convert.ToBase64String(new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(password + username)));
        }

        /// <summary>
        /// Metoden kollar om användaren är behörig till systemet.
        /// </summary>
        /// <param name="username">Användarnamn</param>
        /// <param name="password">Lösenord</param>
        /// <exception cref="SecurityTokenValidationException">If not permitted to login.</exception>
        /// <returns>User - if logged in.</returns>
        public static User Validate(string username, string password)
        {
            var db = new MsSQL();

            if (string.IsNullOrEmpty(username))
            {
                log.Info("Validation failed, missing username.");
                throw new SecurityTokenValidationException("Användarnamn saknas.");
            }

            User dbUser = null;
            username = username.Trim().ToLower();
            try
            {
                dbUser = db.GetUserByUsername(username);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed validate username({0}) and password.", username), ex);
                throw new SecurityTokenException("Fel vid validering av användarnamn och lösenord, var god försök igen senare.");
            }

            if (dbUser != null)
            {
                // Det är okey att logga in om man har ett tomt lösenord, användaren skall då bli uppmanad att sätta ett lösenord.
                // Om ett lösenord finns så skall det så klart matcha det inmatade.
                // Spärrade användare får inte logga in.
                if ((string.IsNullOrEmpty(dbUser.Password) || dbUser.Password == GeneratePasswordHash(dbUser.Username, password.Trim())) && !dbUser.IsDeleted)
                {
                    log.DebugFormat("User successfully validated with id: {0} and username: {1}.", dbUser.Id, username);
                    return dbUser;
                }
                else
                {
                    log.InfoFormat("Failed to validate user, wrong username or password, or user blocked. User with username: {0} and id: {1}.", username, dbUser.Id);
                    throw new SecurityTokenValidationException("Felaktigt användarnamn eller lösenord.");
                }
            }

            log.InfoFormat("Failed to validate user, wrong username or password for user with username: {0}.", username);
            throw new SecurityTokenValidationException("Felaktigt användarnamn eller lösenord.");
        }
    }
}