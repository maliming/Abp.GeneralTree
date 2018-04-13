using System;
using System.Linq;
using Abp.Collections.Extensions;
using Abp.Extensions;
using Abp.GeneralTree.GeneralTree;

namespace Abp.GeneralTree
{
    public class GeneralTreeCodeGenerate : IGeneralTreeCodeGenerate
    {
        private readonly IGeneralTreeCodeGenerateConfiguration _codeGenerateConfiguration;

        public GeneralTreeCodeGenerate(IGeneralTreeCodeGenerateConfiguration codeGenerateConfiguration)
        {
            _codeGenerateConfiguration = codeGenerateConfiguration;
        }

        /// <summary>
        /// Creates code for given numbers.
        /// Example: if numbers are 1,2 then returns "00001.00002";
        /// </summary>
        /// <param name="numbers">Numbers</param>
        public string CreateCode(params int[] numbers)
        {
            if (numbers.IsNullOrEmpty())
            {
                return null;
            }

            return numbers.Select(number => number.ToString(new string('0', _codeGenerateConfiguration.CodeLength)))
                .JoinAsString(".");
        }

        /// <summary>
        /// Merge a child code to a parent code.
        /// Example: if parentCode = "00001", childCode = "00002" then returns "00001.00002".
        /// </summary>
        /// <param name="parentCode">Parent code. Can be null or empty if parent is a root.</param>
        /// <param name="childCode">Child code.</param>
        public string MergeCode(string parentCode, string childCode)
        {
            if (childCode.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(childCode), "childCode can not be null or empty.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return childCode;
            }

            return parentCode + "." + childCode;
        }

        /// <summary>
        /// Merge a child FullName to a parent FullName.
        /// Example: if parentFullName = "00001", childFullName = "00002" then returns "00001-00002".
        /// </summary>
        /// <param name="parentFullName">Parent FullName. Can be null or empty if parent is a root.</param>
        /// <param name="childFullName">Child FullName.</param>
        public string MergeFullName(string parentFullName, string childFullName)
        {
            if (childFullName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(childFullName), "childFullName can not be null or empty.");
            }

            if (parentFullName.IsNullOrEmpty())
            {
                return childFullName;
            }

            return parentFullName + "-" + childFullName;
        }

        /// <summary>
        /// Remove the parent code
        /// Example: if code = "00001.00002.00003" and parentCode = "00001" then returns "00002.00003".
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="parentCode">The parent code.</param>
        public string RemoveParentCode(string code, string parentCode)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return code;
            }

            if (code.Length == parentCode.Length)
            {
                return null;
            }

            return code.Substring(parentCode.Length + 1);
        }

        /// <summary>
        /// Get next code for given code.
        /// Example: if code = "00001.00001" returns "00001.00002".
        /// </summary>
        /// <param name="code">The code.</param>
        public string GetNextCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var parentCode = GetParentCode(code);
            var lastUnitCode = GetLastCode(code);
            return MergeCode(parentCode, CreateCode(Convert.ToInt32(lastUnitCode) + 1));
        }

        /// <summary>
        /// Gets the last code.
        /// Example: if code = "00001.00002.00003" returns "00003".
        /// </summary>
        /// <param name="code">The code.</param>
        public string GetLastCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            return splittedCode[splittedCode.Length - 1];
        }

        /// <summary>
        /// Gets parent code.
        /// Example: if code = "00001.00002.00003" returns "00001.00002".
        /// </summary>
        /// <param name="code">The code.</param>
        public string GetParentCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            if (splittedCode.Length == 1)
            {
                return null;
            }

            return splittedCode.Take(splittedCode.Length - 1).JoinAsString(".");
        }
    }
}