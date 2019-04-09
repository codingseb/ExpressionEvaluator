using NUnit.Framework;
using Shouldly;

namespace CodingSeb.ExpressionEvaluator.Tests
{
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
            // Same sa Script0055 with properties
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
    }
}
