namespace DoenaSoft.SeriesList.Comparer;

public static class NumericStringComparer
{

    public static int Compare(string left, string right)
    {
        var leftCleaned = RemoveLeadingZeros(left);

        var rightCleaned = RemoveLeadingZeros(right);

        if (leftCleaned.Length < rightCleaned.Length)
        {
            return ComparisonResults.LeftLessThanRight;
        }
        else if (leftCleaned.Length > rightCleaned.Length)
        {
            return ComparisonResults.LeftGreaterThanRight;
        }
        else
        {
            var result = StandardStringComparer.Compare(leftCleaned, rightCleaned);

            if (result == ComparisonResults.LeftEqualsRight)
            {
                result = StandardStringComparer.Compare(left, right);
            }

            return result;
        }
    }

    private static string RemoveLeadingZeros(string input)
    {
        var output = input.TrimStart('0');

        if (output.Length == 0)
        {
            output = "0";
        }

        return output;
    }
}