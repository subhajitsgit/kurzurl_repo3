using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Range = Moq.Range;

namespace NUnitTest
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

    public class Bank
    {
        public int Balance { get; set; }
        public Bank(int startingBalance)
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
                Balance -= amount;
                return true;
            }
            return false;
        }
    }

    [TestFixture]
    public class Test
    {
        [Test]
        public void TestArgumentValue()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething("ping")).Returns(true);
            mock.Setup(foo => foo.DoSomething("pong")).Returns(true);

            mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);
            mock.Setup(foo => foo.Add(It.IsInRange<int>(1, 10, Range.Inclusive))).Returns(true);
            mock.Setup(foo => foo.Add(It.Is<int>(x => x % 2 == 0))).Returns(true);
            mock.Setup(foo => foo.DoSomething(It.IsRegex("[a,z]+"))).Returns(false);

            Assert.Multiple(() =>
            {
                Assert.That(mock.Object.DoSomething("123"));
                Assert.That(mock.Object.DoSomething("pong"));
            });
        }

        [Test]
        public void TestArgumentType()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);

            Assert.Multiple(() =>
            {
                Assert.That(mock.Object.DoSomething("123"));
            });
        }

        [Test]
        public void TestRegularExpression()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething(It.IsRegex("[a,z]+"))).Returns(true);

            Assert.Multiple(() =>
            {
                Assert.That(mock.Object.DoSomething("abc"));
            });
        }

        [Test]
        public void TestAddMethod()
        {
            var mock = new Mock<IFoo>();
            //mock.Setup(foo => foo.DoSomething("ping")).Returns(true);
            //mock.Setup(foo => foo.DoSomething("pong")).Returns(true);

            //mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);
            mock.Setup(foo => foo.Add(It.IsInRange<int>(1, 10, Range.Inclusive))).Returns(false);
            //mock.Setup(foo => foo.Add(It.Is<int>(x => x % 2 == 0))).Returns(true);
            //mock.Setup(foo => foo.DoSomething(It.IsRegex("[a,z]+"))).Returns(false);

            Assert.Multiple(() =>
            {
                Assert.That(mock.Object.Add(22));
            });
        }
    }
}
