using Moq;
using Moq.Protected;
using MoqDemo_CreditCardsApps;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {

        private Mock<IFrequentFlyerNumberValidator> mockValidator;
        private CreditCardApplicationEvaluator sut;


        public CreditCardApplicationEvaluatorShould()
        {
            mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.DefaultValue = DefaultValue.Mock;

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        }


        [Fact]
        public void AcceptHighIncomeApplications()
        {
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 100000
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplicants()
        {  
            var application = new CreditCardApplication
            {
                Age = 19
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            //some options below
            
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith("x")))).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsIn("x","y","z"))).Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsInRange("a", "z", Range.Inclusive))).Returns(true);

            mockValidator.Setup(x => x.IsValid(It.IsRegex("[a-z]", RegexOptions.None))).Returns(true); // override the defaults
            
            var application = new CreditCardApplication
            {
                Age = 42,
                GrossAnnualIncome = 19999,
                FrequentFlyerNumber = "a"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {
            mockValidator.Setup(x => x.ValidationMode).Returns(ValidationMode.Quick);
            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void ReferExpiredLicense()
        {
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns(GetLicenceExpiryString);
            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DetailedValidationsForOlderCustomerApplication()
        {
            mockValidator.SetupProperty(x => x.ValidationMode);
            var application = new CreditCardApplication()
            {
                Age = 42
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        string GetLicenceExpiryString()
        {
            return "EXPIRED";
        }

        [Fact]
        public void ShouldValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            var application = new CreditCardApplication();
            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()));
        }

        [Fact]
        public void NotValidateFrequentFlyerNumberForHighIncomeApplications()
        {
            var application = new CreditCardApplication { GrossAnnualIncome = 100_000};
            sut.Evaluate(application);
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications()
        {   
            var application = new CreditCardApplication();
            sut.Evaluate(application);
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Exactly(1)); // Times.Once
        }

        [Fact]
        public void CheckLicenseKeyForLowIncomeApplications()
        {
            var application = new CreditCardApplication { GrossAnnualIncome = 99_000};
            sut.Evaluate(application);
            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey);
        }

        [Fact]
        public void SetDetailedLookupForOlderApplications()
        {
            var application = new CreditCardApplication { Age = 30 };
            sut.Evaluate(application);
            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
        }

        [Fact]
        public void ReferWhenFrequentFlyerValidationError()
        {
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Throws(new Exception("Customer message"));

            var application = new CreditCardApplication { Age = 42 };
            sut.Evaluate(application);
            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void IncrementLookupCount()
        {
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            var application = new CreditCardApplication { Age = 42, FrequentFlyerNumber = "x" };
            sut.Evaluate(application);

            Assert.Equal(1, sut.ValidatorLookupCount);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications_Sequence()
        {
            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            var application = new CreditCardApplication { Age = 25, GrossAnnualIncome = 40_000 };

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }


        [Fact]
        public void ReferFraudRisk()
        {
            Mock<FraudLookup> mockFraudLookup = new Mock<FraudLookup>();

            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>())).Returns(true);
            mockFraudLookup.Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(
                mockValidator.Object,
                mockFraudLookup.Object
                );

            var application = new CreditCardApplication();

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, firstDecision);

        }

        [Fact]
        public void LinqToMocks()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator =
            //    new Mock<IFrequentFlyerNumberValidator>();

            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("Ok");
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            IFrequentFlyerNumberValidator mockValidator = Mock.Of<IFrequentFlyerNumberValidator>
                (
                    validator => 
                    validator.ServiceInformation.License.LicenseKey == "OK" &&
                    validator.IsValid(It.IsAny<string>()) == true
                );

            var sut = new CreditCardApplicationEvaluator(mockValidator);
            var application = new CreditCardApplication { Age = 25} ;

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);

        }
    }
}
