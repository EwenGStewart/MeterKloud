namespace MeterDataLib
{
    public static class ProfileHelpers
    {

        public static decimal[] UnifyLength(decimal[] inputArray, int outputLength, bool useSimpleDivision = false)
        {
            if (inputArray.Length == outputLength) { return inputArray; }
            if (inputArray.Length == 0) { throw new ArgumentException("Input array is empty"); }
            if (inputArray.Length > outputLength)
            {
                decimal[] outputArray = new decimal[outputLength];
                for (int i = 0; i < inputArray.Length; i++)
                {
                    int index = i * outputLength / inputArray.Length;
                    outputArray[index] += inputArray[i];
                }
                return outputArray;
            }
            if (useSimpleDivision || inputArray.Length <= 6)
            {
                return FitCurveWithSimpleDivision(inputArray, outputLength);
            }
            return SmoothCurve(inputArray, outputLength);



        }


        static decimal[] FitCurveWithSimpleDivision(decimal[] inputArray, int outputLength)
        {
            if (inputArray.Length == outputLength) { return inputArray; }
            if (inputArray.Length == 0) { throw new ArgumentException("Input array is empty"); }
            if (inputArray.Length > outputLength) { throw new ArgumentException("Input array is longer than output length"); }
            if (outputLength <= 0) { throw new ArgumentException("Output length is zero or negative"); }
            var outputValuesPerInput = outputLength / inputArray.Length;
            if (outputValuesPerInput * inputArray.Length != outputLength)
            {
                throw new ArgumentException("The output length must be a multiple of the input length - eg 48 (30min) to 288 ( 5min) ");
            }

            decimal[] outputArray = new decimal[outputLength];

            // for each point in the input we will have outputValuesPerInput points in the output - we need to ensure the sum of the points in the output is the same as the input
            // we adjust by simple division 

            for (int i = 0; i < inputArray.Length; i++)
            {
                decimal inputValue = Math.Round(inputArray[i], 3);
                decimal adjustment = Math.Round(inputValue / outputValuesPerInput, 3);
                int outputIndex = i * outputValuesPerInput;
                decimal newTotal = 0;
                for (int j = 0; j < outputValuesPerInput; j++)
                {
                    outputArray[outputIndex + j] = adjustment;
                    newTotal += outputArray[outputIndex + j];
                }
                if (newTotal != inputValue)
                {
                    int j = 0;
                    while (newTotal != inputValue)
                    {
                        if (newTotal < inputValue)
                        {
                            outputArray[outputIndex + j] += 0.001m;
                            newTotal += 0.001m;
                        }
                        else
                        {
                            outputArray[outputIndex + j] -= 0.001m;
                            newTotal -= 0.001m;
                        }
                        j++;
                        if (j >= outputValuesPerInput) { j = 0; }
                    }
                }
            }
            return outputArray;
        }



        static decimal[] SmoothCurve(decimal[] inputArray, int outputLength)
        {
            if (inputArray.Length == outputLength) { return inputArray; }
            if (inputArray.Length == 0) { throw new ArgumentException("Input array is empty"); }
            if (inputArray.Length > outputLength) { throw new ArgumentException("Input array is longer than output length"); }
            if (outputLength <= 0) { throw new ArgumentException("Output length is zero or negative"); }
            var outputValuesPerInput = outputLength / inputArray.Length;
            if (outputValuesPerInput * inputArray.Length != outputLength)
            {
                throw new ArgumentException("The output length must be a multiple of the input length - eg 48 (30min) to 288 ( 5min) ");
            }

            int inputLength = inputArray.Length;




            double[] inputX = Enumerable.Range(0, inputLength).Select(i => ((double)i + 0.5) * outputValuesPerInput).ToArray();
            double[] inputY = inputArray.Select(y => (double)y / outputValuesPerInput).ToArray();

            var interpolatedResult = Interpolation.Interpolate1D(inputX, inputY, outputLength);
            double[] outputY = interpolatedResult.ys;
            decimal[] smoothedArray = new decimal[outputLength];
            for (int i = 0; i < outputLength; i++)
            {
                if( i< outputY.Length)
                {
                    smoothedArray[i] = (decimal)Math.Round(outputY[i], 3);
                }
                else
                {
                    smoothedArray[i] = (decimal)Math.Round(outputY[outputY.Length-1], 3);
                }
            }


            // Adjust the smoothed array to ensure the sum is the same as the input array
            decimal inputSum = inputArray.Sum();
            decimal smoothedSum = smoothedArray.Sum();
            decimal adjustmentFactor = smoothedSum == 0 ? 1 : inputSum / smoothedSum;

            // adjust so that the area under the curve is the same as the input array
            for (int i = 0; i < smoothedArray.Length; i++)
            {
                smoothedArray[i] = Math.Round(smoothedArray[i] * adjustmentFactor, 3);
            }
            // add another adjustment such that the sum of each input value is the same for the same set of point in the output - for each input there will be outputValuesPerInput output values
            for (int i = 0; i < inputLength; i++)
            {
                decimal inputValue = Math.Round(inputArray[i], 3);
                decimal outputValue = 0;
                int outputIndex = i * outputValuesPerInput;
                for (int j = 0; j < outputValuesPerInput; j++)
                {
                    outputValue += smoothedArray[outputIndex + j];
                }
                decimal adjustment = outputValue == 0 ? 1 : inputValue / outputValue;
                decimal newTotal = 0;
                for (int j = 0; j < outputValuesPerInput; j++)
                {
                    smoothedArray[outputIndex + j] = Math.Round(smoothedArray[outputIndex + j] * adjustment, 3);
                    newTotal += smoothedArray[outputIndex + j];
                }
                if (newTotal != inputValue)
                {
                    int j = 0;
                    while (newTotal != inputValue)
                    {
                        if (newTotal < inputValue)
                        {
                            smoothedArray[outputIndex + j] += 0.001m;
                            newTotal += 0.001m;
                        }
                        else
                        {
                            smoothedArray[outputIndex + j] -= 0.001m;
                            newTotal -= 0.001m;
                        }
                        j++;
                        if (j >= outputValuesPerInput) { j = 0; }
                    }
                }

            }
            return smoothedArray;
        }




    }



}
