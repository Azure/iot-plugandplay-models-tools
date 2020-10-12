using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.DigitalTwins.Validator.Exceptions;

namespace Azure.DigitalTwins.Validator
{
    public static partial class Validations
    {

        public async static Task<bool> ScanForReservedWords(this FileInfo fileInfo)
        {
            var fileText = await File.ReadAllTextAsync(fileInfo.FullName);
            return ScanForReservedWords(fileText);
        }

        public static bool ScanForReservedWords(string fileText)
        {
            var reservedRegEx = new Regex("Microsoft|Azure", RegexOptions.IgnoreCase);
            var ids = FindAllIds(fileText, (id) =>
            {
                if (reservedRegEx.IsMatch(id))
                {
                    return false;
                }
                return true;
            });
            var invalidIds = ids.Where(id => !id.Value);
            if (invalidIds.Any())
            {
                throw new ReservedWordException(invalidIds.Select(id => id.Key));
            }
            return true;
        }
    }
}
