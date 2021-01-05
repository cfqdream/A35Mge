using A35Mge.Database;
using A35Mge.IDS.Ids;
using A35Mge.IDS.ValidateExtension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A35Mge.IDS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            var config = Configuration.GetSection("Connection");
            services.AddDbContext<A35MgeDbContext>(options => options.UseMySql(config?.Value ?? string.Empty, mysql =>
            {
                var builder = mysql
                 .MigrationsAssembly(System.Reflection.Assembly.Load("A35Mge.MySqlDatabase").FullName)
                 .EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
            }));
            services.AddIdentityServer(options =>
            {
                //����ͨ����������ָ����¼·����Ĭ�ϵĵ�½·����/account/login
                options.UserInteraction.LoginUrl = "/Account/Login";
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
             //���֤����ܷ�ʽ��ִ�и÷����������ж�tempkey.rsa֤���ļ��Ƿ���ڣ���������ڵĻ����ʹ���һ���µ�tempkey.rsa֤���ļ���������ڵĻ�����ʹ�ô�֤���ļ���
             .AddDeveloperSigningCredential()
             //���ܱ�����Api��Դ��ӵ��ڴ���
             .AddInMemoryApiResources(IdsConfig.GetApiResources())
             //�����Ϣ��Դ
             .AddInMemoryIdentityResources(IdsConfig.GetIdentityResources())
             //����Զ���ͻ���
             .AddClientStore<ClientStore>()
             //����Զ����˺�����ķ���
             .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
             //����΢����֤��½�ķ���
             //.AddExtensionGrantValidator<WechatLoginValidator>()
             .AddInMemoryApiScopes(IdsConfig.ApiScopes);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
