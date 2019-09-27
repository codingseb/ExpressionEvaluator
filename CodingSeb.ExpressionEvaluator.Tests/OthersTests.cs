using NUnit.Framework;
using Shouldly;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    [TestFixture]
    public class OthersTests
    {
        [Test]
        public void RealNestedStructAssignationToSeeHowItWorksScript0055()
        {
            // The real version of Script0055
            StructForTest2 otherStruct;

            otherStruct.AnOtherIntValue = 5;
            otherStruct.nestedStruct = new StructForTest1()
            {
                myIntvalue = 8,
                myStringValue = "Hey"
            };

            otherStruct.nestedStruct.myIntvalue = 9;

            $"Result {otherStruct.nestedStruct.myStringValue} {otherStruct.nestedStruct.myIntvalue}, {otherStruct.AnOtherIntValue}".ShouldBe("Result Hey 9, 5");
        }

        [Test]
        public void RealNestedStructByPropertyAssignationToSeeHowItWorks()
        {
            // Same as Script0055 with properties
            StructForTest4 otherStruct = new StructForTest4();

            otherStruct.AnOtherIntValue = 5;
            otherStruct.NestedStruct = new StructForTest3()
            {
                MyIntvalue = 8,
                MyStringValue = "Hey"
            };

            // Do not compile 
            //otherStruct.NestedStruct.MyIntvalue = 9;

            $"Result {otherStruct.NestedStruct.MyStringValue} {otherStruct.NestedStruct.MyIntvalue}, {otherStruct.AnOtherIntValue}".ShouldBe("Result Hey 8, 5");
        }

        [Test]
        public void RealNestedStructInClassAssignationToSeeHowItWorksScript0056()
        {
            // The real version of Script0056
            ClassStructContainer classStructContainer = new ClassStructContainer()
            {
                nestedStructField = new StructForTest1()
                {
                    myIntvalue = 8,
                    myStringValue = "Hey"
                }
            };

            classStructContainer.nestedStructField.myIntvalue = 9;

            $"Result {classStructContainer.nestedStructField.myStringValue} {classStructContainer.nestedStructField.myIntvalue}".ShouldBe("Result Hey 9");
        }

        [Test]
        public void RealNestedStructInClassPropertyAssignationToSeeHowItWorks()
        {
            ClassStructContainer classStructContainer = new ClassStructContainer()
            {
                NestedStructProperty = new StructForTest1()
                {
                    myIntvalue = 8,
                    myStringValue = "Hey"
                }
            };

            // Do not compile 
            //classStructContainer.NestedStructProperty.myIntvalue = 9;

            $"Result {classStructContainer.NestedStructProperty.myStringValue} {classStructContainer.NestedStructProperty.myIntvalue}".ShouldBe("Result Hey 8");
        }

        [Test]
        public void RealNestedStructPropertyInClassAttributeAssignationToSeeHowItWorksScript0057()
        {
            ClassStructContainer classStructContainer = new ClassStructContainer()
            {
                nestedStructField2 = new StructForTest3()
                {
                    MyIntvalue = 8,
                    MyStringValue = "Hey"
                }
            };

            classStructContainer.nestedStructField2.MyIntvalue = 9;

            $"Result {classStructContainer.nestedStructField2.MyStringValue} {classStructContainer.nestedStructField2.MyIntvalue}".ShouldBe("Result Hey 9");
        }
    }
}
