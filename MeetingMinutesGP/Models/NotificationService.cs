using MeetingMinutesGP.Hubs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace MeetingMinutesGP.Models
{
    public static class NotificaionService
    {
        static readonly string connString = @"data source=DESKTOP-6LS9NL6\SQLEXPRESS;initial catalog=GP;integrated security=True;";

        internal static SqlCommand command = null;
        internal static SqlDependency dependency = null;


        /// <summary>
        /// Gets the notifications.
        /// </summary>
        /// <returns></returns>
        public static string GetNotification()
        {
            try
            {

                var messages = new List<Notification>();
                using (var connection = new SqlConnection(connString))
                {
                    
                    string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
                    GPEntities db = new GPEntities();
                    User Current_user = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
                    connection.Open();
                    //// Sanjay : Alwasys use "dbo" prefix of database to trigger change event
                    using (command = new SqlCommand(@"SELECT [NotificaionId],[IsRead],[Message],[DetailsURL],[Date],[UserId],[VoteID] FROM [dbo].[Notification] where UserId ='" + Current_user.UserID + "'  ORDER BY Date DESC", connection))
                    {
                        command.Notification = null;

                        if (dependency == null)
                        {
                            dependency = new SqlDependency(command);
                            dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);
                        }

                        if (connection.State == ConnectionState.Closed)
                            connection.Open();

                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            /*messages.Add(item: new Notification
                            {
                                NotificaionId = (int)reader["NotificaionId"],
                                IsRead = (bool)reader["IsRead"],
                                Message = reader["Message"] != DBNull.Value ? (string)reader["Message"] : "",
                                DetailsURL = reader["DetailsURL"] != DBNull.Value ? (string)reader["DetailsURL"] : "",
                                Date = (DateTime)reader["Date"],
                                UserId = (int)reader["UserId"],
                                VoteID = (int)reader["VoteID"] != null ? (int)reader["VoteID"] : 0
                            });*/
                            Notification noti = new Notification();
                            noti.NotificaionId = (int)reader["NotificaionId"];
                            noti.IsRead = (bool)reader["IsRead"];
                            if (reader["Message"] != null)
                            {
                                noti.Message = (string)reader["Message"];
                            }
                            else
                            {
                                noti.Message = "";
                            }
                            if (reader["DetailsURL"] != null)
                            {
                                noti.DetailsURL = (string)reader["DetailsURL"];
                            }
                            else
                            {
                                noti.DetailsURL = "";
                            }
                            noti.Date = (DateTime)reader["Date"];
                            noti.UserId = (int)reader["UserId"];
                            
                            if (noti.DetailsURL == "CurrMeeting/viewMeetingContent")
                            {
                                noti.VoteID = 0;
                            }
                            else
                            {
                                noti.VoteID = (int)reader["VoteID"];
                            }
                            messages.Add(noti);
                        }
                    }

                }
                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(messages);
                return json;

            }
            catch (Exception ex)
            {

                return null;
            }


        }

        private static void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (dependency != null)
            {
                dependency.OnChange -= dependency_OnChange;
                dependency = null;
            }
            if (e.Type == SqlNotificationType.Change)
            {
                MyNewHub.Send();
            }
        }

    }

}