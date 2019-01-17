using System;
using System.Collections.Generic;
using System.Text;

namespace MoqDemo_CreditCardsApps
{
    public interface IServiceInformation
    {
        ILicenseData License { get; set; }
    }
}
