using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Foundation.Mobile.Detection;
using System.Linq;

namespace FiftyOne.Tests.Unit.Mobile.Detection
{
    [TestClass]
    public class MostFrequentFilterTests
    {
        private const int NUMBER_OF_ARRAYS = 10;

        private const int NUMBER_OF_ELEMENTS = ushort.MaxValue / NUMBER_OF_ARRAYS;
    
        private int[] createArray(int size, int firstValue, int increment) {
            int[] array = new int[size];
            int lastValue = firstValue;
            for (int i = 0; i < size; i++) {
                array[i] = lastValue;
                lastValue += increment;
            }
            return array;
        }
    
        [TestMethod]
        [TestCategory("Unit"),TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_AllDuplicates() {
            int[][] arrays = new int[NUMBER_OF_ARRAYS][];
            for (int i = 0; i < arrays.Length; i++) {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS, 0, 1);
            }
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays,  int.MaxValue);
            Console.WriteLine("Completed filter in '{0}ms'",
                            (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == NUMBER_OF_ELEMENTS);
            for (int i = 0; i < arrays[0].Length; i++) {
                Assert.IsTrue(filter[i].Equals(arrays[0][i]));
            }
        }
    
        [TestMethod]
        [TestCategory("Unit"), TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_NoDuplicates() {
            int[][] arrays = new int[NUMBER_OF_ARRAYS][];
            int startValue = 1;
            for (int i = 0; i < arrays.Length; i++) {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS, startValue, 1);
                startValue += arrays[i].Length;
            }
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays, int.MaxValue);
            Console.WriteLine("Completed filter in '{0}ms'",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == 
                    NUMBER_OF_ELEMENTS * NUMBER_OF_ARRAYS);
            int lastValue = 0;
            for (int i = 0; i < filter.Count; i++) {
                Assert.IsTrue(lastValue < filter[i]);
                lastValue = filter[i];
            }
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_OneDuplicateArray() {
            const int DUPLICATE_ARRAY_INDEX = 1;
            int[][] arrays = new int[NUMBER_OF_ARRAYS][];
            int startValue = 1;
            for (int i = 0; i < arrays.Length - 1; i++) {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS, startValue, 1);
                startValue += arrays[i].Length;
            }
            arrays[arrays.Length - 1] = arrays[DUPLICATE_ARRAY_INDEX];
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays, int.MaxValue);
            Console.WriteLine("Completed filter in '{0}ms'",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == NUMBER_OF_ELEMENTS);
            for (int i = 0; i < arrays[DUPLICATE_ARRAY_INDEX].Length; i++)
            {
                Assert.IsTrue(filter[i] == arrays[DUPLICATE_ARRAY_INDEX][i]);
            }
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_OneDuplicateValue() {
            int[][] arrays = new int[NUMBER_OF_ARRAYS][];
            int startValue = 1;
            for (int i = 0; i < arrays.Length - 1; i++) {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS, startValue, 1);
                startValue += arrays[i].Length;
            }
            arrays[arrays.Length - 1] = new int[] { 
                arrays[0][NUMBER_OF_ELEMENTS / 2] };
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays, int.MaxValue);
            Console.WriteLine("Completed filter in '{0}ms'",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == 1);
            Assert.IsTrue(filter[0] == arrays[arrays.Length - 1][0]);
        }
    
        [TestMethod]
        [TestCategory("Unit"), TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_MultipleDuplicateValue() {
            int[][] arrays = new int[NUMBER_OF_ARRAYS][];
            for (int i = 0; i < arrays.Length - 1; i++) {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS, 0, 1);
            }
            arrays[arrays.Length - 1] = createArray(NUMBER_OF_ELEMENTS / 5, 0, 5);
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays, int.MaxValue);
            Console.WriteLine("Completed filter in '{0}ms'",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == NUMBER_OF_ELEMENTS / 5);
            for (int i = 0; i < arrays[arrays.Length - 1].Length; i++) {
                Assert.IsTrue(filter[i] == arrays[arrays.Length - 1][i]);
            }
        }    

        [TestMethod]
        [TestCategory("Unit"), TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_DifferentLengthDuplicateValue() {
            int[][] arrays = new int[NUMBER_OF_ARRAYS][];
            for (int i = 0; i < arrays.Length; i++) {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS * (arrays.Length - i), 0, 1);
            }
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays, int.MaxValue);
            Console.WriteLine("Completed filter in '{0}ms'",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == NUMBER_OF_ELEMENTS);
            for (int i = 0; i < arrays[arrays.Length - 1].Length; i++) {
                Assert.IsTrue(filter[i] == arrays[arrays.Length - 1][i]);
            }
        }    

        [TestMethod]
        [TestCategory("Unit"), TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_SingleList() {
            int[][] arrays = new int[1][];
            for (int i = 0; i < arrays.Length; i++) {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS, 0, 1);
            }
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays, int.MaxValue);
            Console.WriteLine("Completed filter in '{0}ms'",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == NUMBER_OF_ELEMENTS);
            for (int i = 0; i < arrays[0].Length; i++) {
                Assert.IsTrue(filter[i] == arrays[0][i]);
            }
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("MostFrequentFilter")]
        public void MostFrequentFilter_MaxResults()
        {
            int[][] arrays = new int[4][];
            int startValue = 1;
            for (int i = 2; i >= 0; i -= 2)
            {
                arrays[i] = createArray(NUMBER_OF_ELEMENTS, startValue, 1);
                arrays[i + 1] = createArray(NUMBER_OF_ELEMENTS, startValue, 1);
                startValue += arrays[i].Length;
            }
            var startTime = DateTime.UtcNow;
            var filter = new Controller.MostFrequentFilter(arrays, NUMBER_OF_ELEMENTS / 10);
            Console.WriteLine("Completed filter in '{0}ms'",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            Assert.IsTrue(filter.Count == NUMBER_OF_ELEMENTS / 10);
            for (int i = 0; i < filter.Count - 1; i++)
            {
                Assert.IsTrue(filter[i] < filter[i+1]);
            }
        }
    }
}
