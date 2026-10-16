using System.Windows;
using Ap2Master.App.Services;
using Ap2Master.App.ViewModels;
using Ap2Master.Application.Abstractions;
using Ap2Master.Domain;
using Ap2Master.Infrastructure;
using Ap2Master.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Ap2Master.App;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var services = new ServiceCollection();
        services.AddAp2Master();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IThemeService>(sp => sp.GetRequiredService<ThemeService>());
        services.AddSingleton<NavigationService>();
        services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<LernplanViewModel>();
        services.AddSingleton<KatalogViewModel>();
        services.AddSingleton<LernkartenViewModel>();
        services.AddSingleton<AufgabenViewModel>();
        services.AddSingleton<SqlLabViewModel>();
        services.AddSingleton<PseudocodeViewModel>();
        services.AddSingleton<UmlViewModel>();
        services.AddSingleton<ProjektViewModel>();
        services.AddSingleton<CoverageViewModel>();
        services.AddSingleton<NetzSecurityViewModel>();
        services.AddSingleton<WiSoViewModel>();
        services.AddSingleton<ProbeklausurViewModel>();
        services.AddSingleton<FehlerbuchViewModel>();
        services.AddSingleton<StatistikViewModel>();
        services.AddSingleton<InhalteViewModel>();
        services.AddSingleton<EinstellungenViewModel>();

        var provider = services.BuildServiceProvider();
        Services = provider;

        using (var scope = provider.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IAppDatabase>().InitializeAsync();
            var sql = scope.ServiceProvider.GetRequiredService<ISqlLabService>();
            await sql.ResetAsync();
            var settings = await UiHelpers.Settings(scope.ServiceProvider.GetRequiredService<AppDbContext>());
            Ap2Master.App.Localization.Loc.Current.SetLanguage(settings.UiLanguage);
            provider.GetRequiredService<ThemeService>().Apply(settings);
        }

        var nav = provider.GetRequiredService<NavigationService>();
        nav.Register(AppSection.Dashboard, () => provider.GetRequiredService<DashboardViewModel>());
        nav.Register(AppSection.Lernplan, () => provider.GetRequiredService<LernplanViewModel>());
        nav.Register(AppSection.Themenkatalog, () => provider.GetRequiredService<KatalogViewModel>());
        nav.Register(AppSection.Lernkarten, () => provider.GetRequiredService<LernkartenViewModel>());
        nav.Register(AppSection.Aufgabentrainer, () => provider.GetRequiredService<AufgabenViewModel>());
        nav.Register(AppSection.SqlLabor, () => provider.GetRequiredService<SqlLabViewModel>());
        nav.Register(AppSection.PseudocodeTrainer, () => provider.GetRequiredService<PseudocodeViewModel>());
        nav.Register(AppSection.UmlErTrainer, () => provider.GetRequiredService<UmlViewModel>());
        nav.Register(AppSection.ProjektmanagementLabor, () => provider.GetRequiredService<ProjektViewModel>());
        nav.Register(AppSection.CoverageTrainer, () => provider.GetRequiredService<CoverageViewModel>());
        nav.Register(AppSection.NetzwerkSecurityTrainer, () => provider.GetRequiredService<NetzSecurityViewModel>());
        nav.Register(AppSection.WiSoTrainer, () => provider.GetRequiredService<WiSoViewModel>());
        nav.Register(AppSection.Probeklausuren, () => provider.GetRequiredService<ProbeklausurViewModel>());
        nav.Register(AppSection.Fehlerbuch, () => provider.GetRequiredService<FehlerbuchViewModel>());
        nav.Register(AppSection.Statistiken, () => provider.GetRequiredService<StatistikViewModel>());
        nav.Register(AppSection.InhalteVerwalten, () => provider.GetRequiredService<InhalteViewModel>());
        nav.Register(AppSection.Einstellungen, () => provider.GetRequiredService<EinstellungenViewModel>());

        var main = new MainWindow { DataContext = provider.GetRequiredService<MainViewModel>() };
        MainWindow = main;
        main.Show();
        nav.Navigate(AppSection.Dashboard);
    }
}
