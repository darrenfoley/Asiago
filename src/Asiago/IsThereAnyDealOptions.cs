using System.ComponentModel.DataAnnotations;

namespace Asiago
{
    internal class IsThereAnyDealOptions
    {
        [Required] // TODO: not working - look into this
        public string ApiKey { get; set; } = null!;
    }
}
