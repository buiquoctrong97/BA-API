using System;
namespace ApiBA.Options
{
	public class Authorization
	{
        public enum Roles
        {
            Administrator,
            Moderator,
            User,
            AirAvailabilityTransit,
            CreateBooking,
            CreateBookingPaynow,
            ConfirmPrice,
            BookingPaynow,
            ListBaggage,
            RetrieveBooking,
        }
        public const string default_username = "administrator";
        public const string default_email = "user@secureapi.com";
        public const string default_password = "Pa$$w0rd.";
        public const Roles default_role = Roles.User;

    }
}

