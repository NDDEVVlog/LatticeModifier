public static class StringExtensions {
    /// <summary>
    /// Computes the FNV-1a hash for the input string. 
    /// The FNV-1a hash is a non-cryptographic hash function known for its speed and good distribution properties.
    /// Useful for creating Dictionary keys instead of using strings.
    /// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
    /// </summary>
    /// <param name="str">The input string to hash.</param>
    /// <returns>An integer representing the FNV-1a hash of the input string.</returns>
    public static int ComputeFNV1aHash(this string str) {
        uint hash = 2166136261;
        foreach (char c in str) {
            hash = (hash ^ c) * 16777619;
        }
        return unchecked((int)hash);
    }
    /// <summary>
    ///  Function to clean the name of the object by removing any number in parentheses
    /// </summary>
    /// <param name="originalName">The input name object.</param>
    /// <returns></returns>
    public static string RemoveNumberSuffix(string originalName)
    {
        // Check if the name ends with a number in parentheses
        int index = originalName.LastIndexOf(" (");
        if (index >= 0)
        {
            return originalName.Substring(0, index); // Return the name without the number
        }
        return originalName; // Return the original name if no number found
    }
}