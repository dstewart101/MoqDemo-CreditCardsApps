using Moq;
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
                new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);

            mockFrequentFlyerNumber.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockFrequentFlyerNumber.Object);


            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }
    }
}
