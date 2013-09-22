using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Text;
using FiberKartan;
using System.Security.Cryptography;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan.Admin.Security
{
    //http://www.mattwrock.com/post/2009/10/14/Implementing-custom-Membership-Provider-and-Role-Provider-for-Authinticating-ASPNET-MVC-Applications.aspx
    public class AdminMemberProvider : MembershipProvider
    {
        #region Unimplemented MembershipProvider Methods

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        #endregion

        //IUserRepository _repository;

        public AdminMemberProvider() //: this(null)
        {
        }

        /*public AdminMemberProvider(IUserRepository repository) : base()
        {
            _repository = repository ?? UserRepositoryFactory.GetRepository();
        }*/

        public User User
        {
            get;
            private set;
        }

        public User CreateUser(string fullName, string passWord, string email)
        {
            return (null);
        }

        /// <summary>
        /// Metoden kollar om användaren är behörig till systemet.
        /// </summary>
        /// <param name="username">Användarnamn</param>
        /// <param name="password">Lösenord</param>
        /// <returns></returns>
        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username.Trim())) return false;

            //string hash = EncryptPassword(password.Trim());
            var fiberDb = new FiberDataContext();
            var dbUser = fiberDb.Users.Where(u => u.Username == username.Trim().ToLower()).SingleOrDefault();

            if (dbUser != null)
            {
                User user = new User(dbUser.Id, dbUser.Username, dbUser.Name, dbUser.Password);
                user.Description = dbUser.Description;
                user.LastLoggedOn = dbUser.LastLoggedOn.HasValue ? dbUser.LastLoggedOn.Value : DateTime.MinValue;

                // Det är okey att logga in om man har ett tomt lösenord, användaren skall då bli uppmanad att sätta ett lösenord.
                // Om ett lösenord finns så skall det så klart matcha det inmatade.
                // Spärrade användare får inte logga in.
                if ((string.IsNullOrEmpty(user.Password) || user.Password == AdminMemberProvider.GeneratePasswordHash(dbUser.Username, password.Trim())) && !dbUser.IsDeleted)
                {
                    User = user;
                    return true;
                }
            }

            Utils.Log("Felaktig inloggning med användarnamn \"" + username + "\".", System.Diagnostics.EventLogEntryType.FailureAudit, 112);

            return false;
        }

        /// <summary>
        /// Genererar ett unikt hashvärde utifrån användarnamnet och lösenordet.
        /// </summary>
        /// <param name="username">Användarnamn</param>
        /// <param name="password">Lösenord</param>
        /// <returns>Unikt hashvärde</returns>
        internal static string GeneratePasswordHash(string username, string password)
        {
            return Convert.ToBase64String(new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(password + username)));
        }
    }
}