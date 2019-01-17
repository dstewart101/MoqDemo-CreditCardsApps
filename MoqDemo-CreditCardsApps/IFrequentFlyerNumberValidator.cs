using MoqDemo_CreditCardsApps;
using System;

namespace CreditCardApplications
{
    public interface IFrequentFlyerNumberValidator
    {
        bool IsValid(string frequentFlyerNumber);
        void IsValid(string frequentFlyerNumber, out bool isValid);

        IServiceInformation ServiceInformation { get; }

        ValidationMode ValidationMode { get; set; }
    }
}