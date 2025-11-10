using NUnit;
using NUnit.Framework;
namespace NUnitTest
{
    public class Bar
    {

    }

    public interface IBaz
    {
        string Name();
    }

    public class BankAccount
    {
        public interface IFoo
        {
            bool DoSomething(string value);
            string ProcessString(string value);
            bool TryParse(string value, out string outputValue);
            bool Submit(ref Bar bar);
            int GetCount();
            bool Add(int amount);

            string Name { get; set; }
            IBaz SomeBaz { get; }
            int SomeOtherProperty { get; set; }

        }
        public int Balance { get; set; }


        public BankAccount(int startingBalance)
        {
            Balance = startingBalance;
        }

        public void Deposit(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "Deposit amount must be positive", nameof(amount));
            Balance += amount;
                
        }

        public bool Withdraw(int amount)
        {
            if (Balance >= amount)
            {
                Balance -=amount;
                return true;
            }
            return false;
        }
    }

    [TestFixture]
    public class DataDrivenTests
    {
        private BankAccount ba;

        [SetUp]
        public void SetUp()
        {
            ba = new BankAccount(100);
        }

        [Test]
        [TestCase(50,true,50)]
        [TestCase(100,true,0)]
        [TestCase(1000,false,100)]
        public void TestMultipltWithdrawalScenerios(int amountWithdrawn, bool shouldSucceed, int expectedBalance)
        {
            var result = ba.Withdraw(amountWithdrawn);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(shouldSucceed));
                Assert.That(expectedBalance,Is.EqualTo(ba.Balance));
            });
        }
    }

    [TestFixture]
    public class UnitTest
    {
        [SetUp]
        public void Test()
        {

        }

        [Test]
        public void TestAssert()
        {
            Assert.That(2+2,Is.EqualTo(4));
        }

    }
}





















































































