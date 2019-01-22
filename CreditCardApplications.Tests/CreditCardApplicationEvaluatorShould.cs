using Moq;
using MoqDemo_CreditCardsApps;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        [Fact]
        public void AcceptHighIncomeApplications()
        {

            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber = 
                new Mock<IFrequentFlyerNumberValidator>();

            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);
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
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey);
            mockFrequentFlyerNumber.DefaultValue = DefaultValue.Mock;
            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);

      
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
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            //mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockFrequentFlyerNumber.Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith("x")))).Returns(true);
            //mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsIn("x","y","z"))).Returns(true);
            //mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsInRange("a", "z", Range.Inclusive))).Returns(true);

            mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsRegex("[a-z]", RegexOptions.None))).Returns(true);
            mockFrequentFlyerNumber.DefaultValue = DefaultValue.Mock;
            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);


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
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("GRAND");
            mockFrequentFlyerNumber.Setup(x => x.ValidationMode).Returns(ValidationMode.Quick);
            
            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);


            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void ReferExpiredLicense()
        {
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey).Returns(GetLicenceExpiryString);

            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DetailedValidationsForOlderCustomerApplication()
        {
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("GRAND");
            mockFrequentFlyerNumber.SetupProperty(x => x.ValidationMode);

            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);


            var application = new CreditCardApplication()
            {
                Age = 42
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, mockFrequentFlyerNumber.Object.ValidationMode);
        }

        string GetLicenceExpiryString()
        {
            return "EXPIRED";
        }

        [Fact]
        public void ShouldValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("GRAND");
            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);


            //var application = new CreditCardApplication
            //{
            //    FrequentFlyerNumber = "flyerNumber"
            //};

            var application = new CreditCardApplication();



            sut.Evaluate(application);

            mockFrequentFlyerNumber.Verify(x => x.IsValid(It.IsAny<string>()));

        }

        //[Fact]
        //public void ShouldValidateFrequentFlyerNumberForLowIncomeApplications_CustomMessage()
        //{
        //    Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
        //        new Mock<IFrequentFlyerNumberValidator>();

        //    mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("GRAND");
        //    var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);
        //    var application = new CreditCardApplication();

        //    sut.Evaluate(application);
        //    mockFrequentFlyerNumber.Verify(x => x.IsValid(It.IsNotNull<string>()), "Frequent flyer number passed should not be null");
        //}

        [Fact]
        public void NotValidateFrequentFlyerNumberForHighIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("GRAND");

            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);

            var application = new CreditCardApplication { GrossAnnualIncome = 100_000};

            sut.Evaluate(application);

            mockFrequentFlyerNumber.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);

        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockFrequentFlyerNumber =
                new Mock<IFrequentFlyerNumberValidator>();

            mockFrequentFlyerNumber.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("GRAND");
            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);

            var application = new CreditCardApplication();

            sut.Evaluate(application);

            mockFrequentFlyerNumber.Verify(x => x.IsValid(It.IsAny<string>()), Times.Exactly(1)); // Times.Once

        }
    }
}
