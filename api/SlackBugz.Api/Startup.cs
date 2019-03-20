using System;
using System.Linq;
using System.Net.Http;
using SlackBugz.Api.Config;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static SlackBugz.Api.Config.FogBugzConfig;

namespace SlackBugz.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
			services.AddAuthorization(options =>
			{				
				options.AddPolicy(Constants.SlackSignedSecretPolicyName, policy =>
					policy.Requirements.Add(new SlackSignedSecretRequirement()));
			});

			services.AddTransient<HttpClient>();

			services.AddSingleton(new FogBugzConfig(
				orgName: Configuration["AppSettings:FogBugz:OrgName"],
				getApiToken: () => Configuration["AppSettings:FogBugz:ApiToken"],
				apiUrl: Configuration["AppSettings:FogBugz:ApiUrl"],
				linkInfos: Configuration.GetSection("AppSettings:FogBugz:ApiUrl:Links")
					.GetChildren()
					.Select(kiddo => new SlackLinkConfig(
						displayName: kiddo["DisplayName"],
						baseUrl: kiddo["BaseUrl"],
						color: kiddo["Color"]))
					.ToArray()));

			services.AddSingleton(new SlackConfig(
				workspace: Configuration["AppSettings:Slack:Workspace"],
				shortSlashCommands: Configuration.GetSection("AppSettings:Slack:ShortSlashCommands")
					.ParseFlatValueArray(),
				longSlashCommands: Configuration.GetSection("AppSettings:Slack:LongSlashCommands")
					.ParseFlatValueArray()));

			services.AddSingleton(new SlackSignedSecretConfig(
				getConfiguredSecret: () => Configuration["AppSettings:SlackSigningSecret"],
				getUtcNow: () => DateTime.UtcNow));

			services.AddHangfire(x => x.UseStorage(new MemoryStorage()));
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseMvc();

			app.UseHangfireDashboard();
			app.UseHangfireServer();
		}

		
	}

	public static class ConfigurationExtensions
	{
		// https://stackoverflow.com/questions/41329108/asp-net-core-get-json-array-using-iconfiguration#comment86827525_41330941
		public static string[] ParseFlatValueArray(this IConfigurationSection configSection)
		{
			return configSection.GetChildren().ToArray().Select(c => c.Value).ToArray();
		}
	}
}
