using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace BMES.Infrastructure.Persistence
{
    public class ObjectToJsonConverter : ValueConverter<object?, string?>
    {
        public ObjectToJsonConverter()
            : base(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<object?>(v)
            )
        {
        }
    }
}
