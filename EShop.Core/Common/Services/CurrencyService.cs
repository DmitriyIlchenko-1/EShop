using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace EShop.Core.Common.Services
{
	public class CurrencyService : ICurrencyService
	{
		private readonly IConfiguration _configuration;

		public CurrencyService(IConfiguration configuration)
		{
			this._configuration = configuration;
		}

		public string FormatCurrency(decimal value)
		{
			int num = ConfigurationBinder.GetValue<int>(this._configuration, "Global.CurrencyDecimalPlace");
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
			defaultInterpolatedStringHandler.AppendLiteral("C");
			defaultInterpolatedStringHandler.AppendFormatted<int>(num);
			return value.ToString(defaultInterpolatedStringHandler.ToStringAndClear());
		}
	}
}