using System;

namespace AspNetCore.Identity.MongoDb.Models
{
    internal class TwoFactorRecoveryCode
    {
        public string Code { get; set; }

        public bool Redeemed { get; set; }
    }
}