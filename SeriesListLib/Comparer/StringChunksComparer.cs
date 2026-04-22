namespace DoenaSoft.SeriesList.Comparer;

internal static class StringChunksComparer
{
    public static int Compare(string left, string right)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return StandardStringComparer.Compare(left, right);
            }

            if (DateTime.TryParse(left, out DateTime leftTimestamp)
                && DateTime.TryParse(right, out DateTime rightTimestamp))
            {
                return DateTime.Compare(leftTimestamp, rightTimestamp);
            }

            var leftChunks = GetChunks(left);

            var rightChunks = GetChunks(right);

            var result = CompareChunks(leftChunks, rightChunks);

            return result;
        }
        catch
        {
            return ComparisonResults.LeftEqualsRight;
        }
    }

    private static int CompareChunks(List<string> leftChunks, List<string> rightChunks)
    {
        var maximumChunkCount = Math.Max(leftChunks.Count, rightChunks.Count);

        for (var chunkIndex = 0; chunkIndex < maximumChunkCount; chunkIndex++)
        {
            var leftChunk = leftChunks.ElementAtOrDefault(chunkIndex);

            var rightChunk = rightChunks.ElementAtOrDefault(chunkIndex);

            if (rightChunk == null)
            {
                return ComparisonResults.LeftGreaterThanRight;
            }
            else if (leftChunk == null)
            {
                return ComparisonResults.LeftLessThanRight;
            }
            else if (IsNumeric(leftChunk) && IsNumeric(rightChunk))
            {
                var result = NumericStringComparer.Compare(leftChunk, rightChunk);

                if (result != ComparisonResults.LeftEqualsRight)
                {
                    return result;
                }
            }
            else
            {
                var result = StandardStringComparer.Compare(leftChunk, rightChunk);

                if (result != ComparisonResults.LeftEqualsRight)
                {
                    return result;
                }
            }
        }

        return ComparisonResults.LeftEqualsRight;
    }

    private static List<string> GetChunks(string input)
    {
        var result = new List<string>();

        var output = input.First().ToString();

        var inputAfterFirstChar = input.Substring(1, input.Length - 1);

        foreach (var character in inputAfterFirstChar)
        {
            var lastChar = output.Last();

            var lastCharIsNumber = char.IsNumber(lastChar);

            if (char.IsNumber(character) && lastCharIsNumber)
            {
                output += character;
            }
            else if (!char.IsNumber(character) && !lastCharIsNumber)
            {
                output += character;
            }
            else
            {
                result.Add(output);

                output = character.ToString();
            }
        }

        result.Add(output);

        return result;
    }

    private static bool IsNumeric(string inputString) => char.IsNumber(inputString.First());
}