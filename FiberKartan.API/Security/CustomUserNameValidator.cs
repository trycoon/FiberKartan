using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FiberKartan.Database;
using log4net;

/*
Copyright (c) 2012, Henrik Östman.

This file is part of FiberKartan.

FiberKartan is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

FiberKartan is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with FiberKartan.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace FiberKartan.API.Security
{
    public class CustomUserNameValidator : UserNamePasswordValidator
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
        /// <returns></returns>
        public override void Validate(string username, string password)
        {
            var db = new MsSQL();

            if (string.IsNullOrEmpty(username.Trim()))
            {
                log.Info("Login failed, missing username.");
                throw new SecurityTokenValidationException("Användarnamn saknas.");
            }

            Database.Models.User dbUser = null;
            try
            {
                dbUser = db.GetUserByUsername(username.Trim().ToLower());
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
                    log.InfoFormat("Logged in user with id: {0} and username: {1}.", dbUser.Id, username);
                }
                else
                {
                    log.InfoFormat("User failed to login, wrong username or password, or user blocked. User with username: {0} and id: {1}.", username, dbUser.Id);
                    throw new SecurityTokenValidationException("Felaktigt användarnamn eller lösenord.");
                }
            }

            log.InfoFormat("User failed to login, wrong username or password for user with username: {0}.", username);
            throw new SecurityTokenValidationException("Felaktigt användarnamn eller lösenord.");
        }
    }
}