using System;
using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    // NOTE! MAKE SURE TO NOT LEAK SENSITIVE USER INFORMATION. ALWAYS MAKE SURE THAT REQUESTING USER HAS ACCESS-RIGHTS TO VIEW THIS.
    // MOST OF THE TIMES THE "Creator"-CLASS IS MORE APPROPRIATE TO USE!
    [DataContract(Name = "user")]
    public class User
    {
                /// <summary>
        /// Default constructor
        /// </summary>
        public User()
        {
            // Nothing here.
        }

        /// <summary>
        /// Copy-constructor
        /// </summary>
        /// <param name="dbUser">Database entity to copy</param>
        public User(Internal.User dbUser)
        {
            this.Id = dbUser.Id;
            this.Name = dbUser.Name;
            this.Username = dbUser.Username;
            this.Password = dbUser.Password;
            this.Description = dbUser.Description;
            this.IsDeleted = dbUser.IsDeleted;
            this.Created = dbUser.Created;
            this.LastLoggedOn = dbUser.LastLoggedOn ?? DateTime.MinValue;
            this.LastActivity = dbUser.LastActivity ?? DateTime.MinValue;
            this.IsAdmin = dbUser.IsAdmin;
            this.LastNotificationMessage = dbUser.LastNotificationMessage ?? -1;
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "isDeleted")]
        public bool IsDeleted { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "lastLoggedOn")]
        public DateTime LastLoggedOn { get; set; }

        [DataMember(Name = "lastActivity")]
        public DateTime LastActivity { get; set; }

        [DataMember(Name = "isAdmin")]
        public bool IsAdmin { get; set; }

        [DataMember(Name = "lastNotificationMessage")]
        public int LastNotificationMessage { get; set; }
    }
}
