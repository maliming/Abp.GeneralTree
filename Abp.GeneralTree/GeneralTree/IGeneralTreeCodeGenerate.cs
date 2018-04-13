namespace Abp.GeneralTree.GeneralTree
{
    public interface IGeneralTreeCodeGenerate
    {
        /// <summary>
        /// Creates code for given numbers.
        /// Example: if numbers are 1 then returns "00001"; if numbers are 1,2 then returns "00001.00002";
        /// </summary>
        /// <param name="numbers">Numbers</param>
        string CreateCode(params int[] numbers);

        /// <summary>
        /// Merge a child code to a parent code.
        /// Example: if parentCode = "00001", childCode = "00002" then returns "00001.00002".
        /// </summary>
        /// <param name="parentCode">Parent code. Can be null or empty if parent is a root.</param>
        /// <param name="childCode">Child code.</param>
        string MergeCode(string parentCode, string childCode);

        /// <summary>
        /// Merge a child FullName to a parent FullName.
        /// Example: if parentFullName = "00001", childFullName = "00002" then returns "00001-00002".
        /// </summary>
        /// <param name="parentFullName">Parent FullName. Can be null or empty if parent is a root.</param>
        /// <param name="childFullName">Child FullName.</param>
        string MergeFullName(string parentFullName, string childFullName);

        /// <summary>
        /// Remove the parent code
        /// Example: if code = "00001.00002.00003" and parentCode = "00001" then returns "00002.00003".
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="parentCode">The parent code.</param>
        string RemoveParentCode(string code, string parentCode);

        /// <summary>
        /// Get next code for given code.
        /// Example: if code = "00001.00001" returns "00001.00002".
        /// </summary>
        /// <param name="code">The code.</param>
        string GetNextCode(string code);

        /// <summary>
        /// Gets the last code.
        /// Example: if code = "00001.00002.00003" returns "00003".
        /// </summary>
        /// <param name="code">The code.</param>
        string GetLastCode(string code);

        /// <summary>
        /// Gets parent code.
        /// Example: if code = "00001.00002.00003" returns "00001.00002".
        /// </summary>
        /// <param name="code">The code.</param>
        string GetParentCode(string code);
    }
}