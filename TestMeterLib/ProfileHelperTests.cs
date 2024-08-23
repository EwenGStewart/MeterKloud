using FluentAssertions;
using MeterDataLib;
using MeterDataLib.Query;
using Xunit.Abstractions;

namespace TestMeterLib
{


    public class ProfileHelperTests(ITestOutputHelper Output)
    {



        [Fact]
        public void TestProfileSineCubic()
        {
            var input = GenSineWave(48,3, 10 , 10  );
            input.Length.Should().Be(48);
            var profile =  ProfileHelpers.UnifyLength(input, 288);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(input.Sum());
            for( int i=0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(input[i]);
            }
        }


        [Fact]
        public void TestProfileSineCubicZeroSum()
        {
            var input = GenSineWave(48, 3, 10, 0);
            var profile = ProfileHelpers.UnifyLength(input, 288);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(input.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(input[i]);
            }
        }


        [Fact]
        public void TestProfileSineCubicAllNeg()
        {
            var input = GenSineWave(48, 3, 5, -10.0);
            var profile = ProfileHelpers.UnifyLength(input, 288);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(input.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(input[i]);
            }
        }




        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        [InlineData(-2)]
        [InlineData(Math.PI)]
        public void TestProfileConst( decimal n )
        {
            var input = GenConst(48, n);
            var roundedInput = input.Select(x => Math.Round(x, 3)).ToArray();
            var profile = ProfileHelpers.UnifyLength(input, 288);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(roundedInput.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(roundedInput[i]);
            }
        }

        [Fact]
        public void TestDemoSineWithRandom()
        {
            var input = GenSineWave(48, 3, 10, 10);
            Random random = new Random();
            for (int i = 0; i < 48; i++)
            {
                input[i] += (decimal)(random.NextDouble()  );
            }
            var roundedInput = input.Select(x => Math.Round(x, 3)).ToArray();
            var profile = ProfileHelpers.UnifyLength(input, 288);
            var profileSimple = ProfileHelpers.UnifyLength(input, 288, true );
            profile.Length.Should().Be(288);
            profileSimple.Length.Should().Be(288);
            profile.Sum().Should().Be(roundedInput.Sum());
            profileSimple.Sum().Should().Be(roundedInput.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(roundedInput[i]);
                profileSimple.Skip(i * 6).Take(6).Sum().Should().Be(roundedInput[i]);
            }
            Console.SetOut(new RedirectOutput(Output));
 
            Console.WriteLine("---------------------------");
            for (int i = 0; i < 288; i++)
            {
                Console.WriteLine($"{i}, {profile[i]:0.000}, {profileSimple[i]:0.000}");
            }




        }




        [Fact]
        public void TestProfileSimple()
        {
            var input = GenSineWave(48, 3, 10, 10);
            input.Length.Should().Be(48);
            var profile = ProfileHelpers.UnifyLength(input, 288,true);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(input.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(input[i]);
            }
        }


        [Fact]
        public void TestProfileSimpleZeroSum()
        {
            var input = GenSineWave(48, 3, 10, 0);
            var profile = ProfileHelpers.UnifyLength(input, 288, true);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(input.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(input[i]);
            }
        }


        [Fact]
        public void TestProfileSimpleAllNeg()
        {
            var input = GenSineWave(48, 3, 5, -10.0);
            var profile = ProfileHelpers.UnifyLength(input, 288, true);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(input.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(input[i]);
            }
        }




        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        [InlineData(-2)]
        [InlineData(Math.PI)]
        public void TestProfileConstSimple(decimal n)
        {
            var input = GenConst(48, n);
            var roundedInput = input.Select(x => Math.Round(x, 3)).ToArray();

            var profile = ProfileHelpers.UnifyLength(input, 288,true);
            profile.Length.Should().Be(288);
            profile.Sum().Should().Be(roundedInput.Sum());
            for (int i = 0; i < 48; i++)
            {
                profile.Skip(i * 6).Take(6).Sum().Should().Be(roundedInput[i]);
            }
        }





        static decimal[] GenSineWave(int n , int rounding = 3 , double multiplier = 10 , double offset =10  )
        {
            var result = new decimal[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = (decimal)Math.Round( Math.Sin(2 * Math.PI * i / n) * multiplier + offset , rounding);
            }
            return result; 
        }


 

        static decimal[] GenConst(int n, decimal value )
        {
            var result = new decimal[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = value;
            }
            return result;
        }





    }

}
 