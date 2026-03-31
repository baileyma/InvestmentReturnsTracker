//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Jobs;
//using InvTracker.Models;
//using InvTracker.Utils;

//namespace Benchmarks
//{
//    public class XIRRBenchmark
//    {
//        public HelperMethods _sut;
//        private Payment _payment1;
//        private Payment _payment2;
//        private XirrInput _xirr1;

//        [GlobalSetup]
//        public void Setup()
//        {
//            _sut = new HelperMethods();
//            _payment1 = new Payment()
//            {
//                Id = 1,
//                AccountId = 2,
//                Date = new DateOnly(2024, 4, 2),
//                Amount = 2000
//            };

//            _payment2 = new Payment()
//            {
//                Id = 2,
//                AccountId = 2,
//                Date = new DateOnly(2024, 6, 20),
//                Amount = 4000
//            };

//            _xirr1 = new XirrInput()
//            {
//                StartingAmount = 100000,
//                EndAmount = 15000,
//                Payments = new List<Payment>() { _payment1, _payment2 },
//                EndMonth = 11,
//                EndDay = 5
//            };
//        }

//        [Benchmark(Baseline = true)]
//        [Arguments(_xirr1, 2024)]
//        public double XIRRCalculation(xirr, year)
//        {
//            var
//            // autofixture or another way?

//            var result = _sut.CalculateXirr(sirr, year);

//            return result;
//        }
//    }
//}