using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace DistancesAPI
{
    /// <inheritdoc />
    [PublicAPI]
    public class DelimitedTextInputFormatter : TextInputFormatter
    {
        /// <inheritdoc />
        public DelimitedTextInputFormatter()
        {
            SupportedMediaTypes.Add("text/csv");
            SupportedEncodings.Add(Encoding.ASCII);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        }

        /// <inheritdoc />
        protected override bool CanReadType([CanBeNull] Type type) => type == typeof(string);

        /// <inheritdoc />
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            [NotNull] InputFormatterContext context,
            [CanBeNull] Encoding encoding)
        {
            using (TextReader streamReader = context.ReaderFactory(context.HttpContext.Request.Body, encoding))
            {
                return InputFormatterResult.Success(await streamReader.ReadToEndAsync());
            }
        }
    }
}