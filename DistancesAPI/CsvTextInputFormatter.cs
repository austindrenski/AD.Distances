using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace DistancesAPI
{
    [PublicAPI]
    public class DelimitedTextInputFormatter : TextInputFormatter
    {
        public DelimitedTextInputFormatter()
        {
            SupportedMediaTypes.Add("text/csv");
            SupportedEncodings.Add(Encoding.ASCII);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            using (TextReader streamReader = context.ReaderFactory(context.HttpContext.Request.Body, encoding))
            {
                return InputFormatterResult.Success(await streamReader.ReadToEndAsync());
            }
        }
    }
}