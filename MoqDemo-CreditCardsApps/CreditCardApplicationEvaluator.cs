using System;
using MoqDemo_CreditCardsApps;

namespace CreditCardApplications
{
    public class CreditCardApplicationEvaluator
    {
        private const int AutoReferralMaxAge = 20;
        private const int HighIncomeThreshhold = 100_000;
        private const int LowIncomeThreshhold = 20_000;

        private IFrequentFlyerNumberValidator _frequentFlyerNumberValidator;

        public int ValidatorLookupCount { get; private set; }

        private readonly FraudLookup _fraudLookup;

        public CreditCardApplicationEvaluator(IFrequentFlyerNumberValidator frequentFlyerNumberValidator,
            FraudLookup fraudLookup = null)
        {
            _frequentFlyerNumberValidator = frequentFlyerNumberValidator ?? throw new System.ArgumentNullException(nameof(_frequentFlyerNumberValidator));
            _frequentFlyerNumberValidator.ValidatorLookupPerformed += ValidatorLookUpPerformed;
            _fraudLookup = fraudLookup;
        }

        private void ValidatorLookUpPerformed(object sender, EventArgs e)
        {
            ValidatorLookupCount++;
        }

        public CreditCardApplicationDecision Evaluate(CreditCardApplication application)
        {

            if (_fraudLookup != null && _fraudLookup.IsFraudRisk(application))
            {
                return CreditCardApplicationDecision.ReferredToHumanFraudRisk;
            }

            if (application.GrossAnnualIncome >= HighIncomeThreshhold)
            {
                return CreditCardApplicationDecision.AutoAccepted;
            }

            if (_frequentFlyerNumberValidator.ServiceInformation.License.LicenseKey == "EXPIRED")
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            _frequentFlyerNumberValidator.ValidationMode = application.Age >= 30 ? ValidationMode.Detailed : ValidationMode.Quick;

            bool isValidFrequentFlyerNumber;

            try
            {
                isValidFrequentFlyerNumber = _frequentFlyerNumberValidator.IsValid(application.FrequentFlyerNumber);
            }

            catch (System.Exception)
            {
                // log
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.Age <= AutoReferralMaxAge)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.GrossAnnualIncome < LowIncomeThreshhold)
            {
                return CreditCardApplicationDecision.AutoDeclined;
            }

            return CreditCardApplicationDecision.ReferredToHuman;
        }       
    }
}
