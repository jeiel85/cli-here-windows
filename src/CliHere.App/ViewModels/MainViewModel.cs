using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CliHere.App.Models;
using CliHere.App.Services;
using CliHere.App.Views;

namespace CliHere.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    private readonly LocalizationService _localizationService;
    private readonly ContextMenuRegistryService _contextMenuRegistryService;
    private readonly CliDetectionService _cliDetectionService;
    private readonly UpdateService _updateService;
    private readonly System.Timers.Timer _updateTimer;
    private AppSettings _settings;

    public MainViewModel(
        SettingsService settingsService,
        LocalizationService localizationService,
        CliDefinitionService cliDefinitionService,
        ContextMenuRegistryService contextMenuRegistryService,
        CliDetectionService cliDetectionService)
    {
        _settingsService = settingsService;
        _localizationService = localizationService;
        _contextMenuRegistryService = contextMenuRegistryService;
        _cliDetectionService = cliDetectionService;
        _settings = _settingsService.Load();
        _updateService = new UpdateService();
        _updateTimer = new System.Timers.Timer(TimeSpan.FromHours(24).TotalMilliseconds)
        {
            AutoReset = true,
        };
        _updateTimer.Elapsed += async (_, _) => await BackgroundCheckAsync();

        CliItems = new ObservableCollection<CliItemViewModel>(
            cliDefinitionService.GetAll().Select(cli => new CliItemViewModel
            {
                Definition = cli,
                DetectionResult = _cliDetectionService.Detect(cli),
                IsEnabled = _settings.EnabledCliIds.Count == 0 || _settings.EnabledCliIds.Contains(cli.Id, StringComparer.OrdinalIgnoreCase),
            }));

        ApplyCommand = new RelayCommand(_ => Apply());
        RemoveAllCommand = new RelayCommand(_ => RemoveAll());
        RefreshCommand = new RelayCommand(_ => RefreshDetectionStatus());
        OpenInstallLinkCommand = new RelayCommand(param => OpenLink(param, isInstallLink: true));
        OpenDocsLinkCommand = new RelayCommand(param => OpenLink(param, isInstallLink: false));
        CheckUpdateCommand = new RelayCommand(_ => _ = ManualCheckForUpdateAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<CliItemViewModel> CliItems { get; }
    public ICommand ApplyCommand { get; }
    public ICommand RemoveAllCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand OpenInstallLinkCommand { get; }
    public ICommand OpenDocsLinkCommand { get; }
    public ICommand CheckUpdateCommand { get; }
    public string AppTitle => _localizationService.Translate("App.Title", Language);
    public string LanguageLabel => T("Settings.Language");
    public string TerminalLabel => T("Settings.Terminal");
    public string RunAsAdministratorLabel => T("Settings.RunAsAdministrator");
    public string SupportedCliLabel => T("Section.SupportedCli");
    public string CliColumnIdLabel => T("Cli.Column.Id");
    public string CliColumnNameLabel => T("Cli.Column.Name");
    public string CliColumnCommandLabel => T("Cli.Column.Command");
    public string CliColumnStatusLabel => T("Cli.Column.Status");
    public string CliColumnEnabledLabel => T("Cli.Column.Enabled");
    public string CliColumnActionsLabel => T("Cli.Column.Actions");
    public string LanguageSystemLabel => T("Language.System");
    public string LanguageKoreanLabel => T("Language.Korean");
    public string LanguageEnglishLabel => T("Language.English");
    public string TerminalWindowsTerminalLabel => T("Terminal.WindowsTerminal");
    public string TerminalPowerShellLabel => T("Terminal.PowerShell");
    public string RefreshLabel => T("Action.Refresh");
    public string RemoveAllLabel => T("Action.RemoveAll");
    public string ApplyLabel => T("Action.Apply");
    public string CheckUpdateLabel => T("Action.CheckUpdate");
    public string CurrentVersionLabel => $"{T("Update.CurrentVersionPrefix")} v{UpdateService.CurrentVersion.ToString(3)}";
    public string UpdateCheckLabel { get; private set; } = string.Empty;
    public string InstallLinkLabel => T("Cli.Action.OpenInstallPage");
    public string DocsLinkLabel => T("Cli.Action.OpenDocsPage");
    public string InstalledStatusLabel => T("Cli.Status.Installed");
    public string NotInstalledStatusLabel => T("Cli.Status.NotInstalled");

    public LanguageMode Language
    {
        get => _settings.Language;
        set
        {
            if (_settings.Language == value) return;
            _settings.Language = value;
            OnPropertyChanged();
            NotifyLocalizedPropertiesChanged();
        }
    }

    public TerminalMode Terminal
    {
        get => _settings.Terminal;
        set
        {
            if (_settings.Terminal == value) return;
            _settings.Terminal = value;
            OnPropertyChanged();
        }
    }

    public bool RunAsAdministrator
    {
        get => _settings.RunAsAdministrator;
        set
        {
            if (_settings.RunAsAdministrator == value) return;
            _settings.RunAsAdministrator = value;
            OnPropertyChanged();
        }
    }

    public async Task OnAppStartedAsync()
    {
        _updateTimer.Start();
        await BackgroundCheckAsync();
    }

    private void Apply()
    {
        string appPath = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot resolve executable path.");
        _contextMenuRegistryService.RemoveAll();

        _settings.EnabledCliIds = CliItems.Where(x => x.IsEnabled).Select(x => x.Id).ToList();

        foreach (CliItemViewModel cliItem in CliItems.Where(x => x.IsEnabled))
        {
            string parentMenuLabel = AppTitle;
            string label = _localizationService.Translate("ContextMenu.OpenWith", Language, cliItem.DisplayName);
            _contextMenuRegistryService.RegisterCli(cliItem.Definition, parentMenuLabel, label, appPath);
        }

        _settingsService.Save(_settings);
        MessageBox.Show(_localizationService.Translate("Message.Applied", Language));
    }

    private void RemoveAll()
    {
        _contextMenuRegistryService.RemoveAll();
        MessageBox.Show(_localizationService.Translate("Message.Removed", Language));
    }

    private void RefreshDetectionStatus()
    {
        for (int i = 0; i < CliItems.Count; i++)
        {
            bool isEnabled = CliItems[i].IsEnabled;
            CliDefinition definition = CliItems[i].Definition;
            CliItems[i] = new CliItemViewModel
            {
                Definition = definition,
                DetectionResult = _cliDetectionService.Detect(definition),
                IsEnabled = isEnabled,
            };
        }
    }

    private async Task ManualCheckForUpdateAsync()
    {
        UpdateCheckLabel = T("Update.Checking");
        OnPropertyChanged(nameof(UpdateCheckLabel));

        UpdateService.UpdateInfo? result;
        try
        {
            result = await _updateService.CheckForUpdateAsync();
        }
        catch (UpdateCheckException ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                string message = ex.Kind switch
                {
                    UpdateCheckErrorKind.Network => T("Update.Error.Network"),
                    UpdateCheckErrorKind.Timeout => T("Update.Error.Timeout"),
                    UpdateCheckErrorKind.RateLimit => string.Format(T("Update.Error.RateLimit"), ex.RetryAtLocal ?? "?"),
                    UpdateCheckErrorKind.ApiError => string.Format(T("Update.Error.Api"), ex.StatusCode ?? 0),
                    _ => T("Update.Error.Unknown"),
                };
                MessageBox.Show(message, T("Update.DialogTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            });

            UpdateCheckLabel = string.Empty;
            OnPropertyChanged(nameof(UpdateCheckLabel));
            return;
        }
        catch
        {
            UpdateCheckLabel = string.Empty;
            OnPropertyChanged(nameof(UpdateCheckLabel));
            return;
        }

        if (result is null)
        {
            UpdateCheckLabel = string.Format(T("Update.UpToDate"), UpdateService.CurrentVersion.ToString(3));
            OnPropertyChanged(nameof(UpdateCheckLabel));
            return;
        }

        UpdateCheckLabel = string.Empty;
        OnPropertyChanged(nameof(UpdateCheckLabel));
        await Application.Current.Dispatcher.InvokeAsync(() => OfferUpdate(result));
    }

    private async Task BackgroundCheckAsync()
    {
        try
        {
            UpdateService.UpdateInfo? result = await _updateService.CheckForUpdateAsync();
            if (result is null) return;
            await Application.Current.Dispatcher.InvokeAsync(() => OfferUpdate(result));
        }
        catch
        {
            // Background update check should fail silently.
        }
    }

    private void OfferUpdate(UpdateService.UpdateInfo info)
    {
        string version = info.Version.ToString(3);
        if (IsVersionSkipped(version))
        {
            return;
        }

        UpdateDialog dialog = new(
            info.ReleaseNotes,
            onSkip: () => SkipVersion(version),
            title: T("Update.DialogTitle"),
            skipLabel: T("Update.Skip"),
            updateLabel: T("Update.UpdateNow"),
            errorTitle: T("Update.ErrorTitle"));

        dialog.OnUpdateRequested += () =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    string preparedPath = await _updateService.DownloadAndPrepareUpdateAsync(
                        info.DownloadUrl,
                        info.Sha256Url,
                        (pc, status) =>
                        {
                            string localizedStatus = status switch
                            {
                                "Downloading..." => T("Update.Status.Downloading"),
                                "Verifying..." => T("Update.Status.Verifying"),
                                _ => status,
                            };
                            dialog.UpdateProgress(pc, localizedStatus);
                        });

                    dialog.UpdateProgress(100, T("Update.Restarting"));
                    await Task.Delay(500);
                    _updateService.ApplyPreparedUpdate(preparedPath);
                }
                catch (Exception ex)
                {
                    dialog.ShowError(ex.Message);
                }
            });
        };

        dialog.Show();
    }

    private bool IsVersionSkipped(string version)
        => string.Equals(_settings.SkippedUpdateVersion, version, StringComparison.OrdinalIgnoreCase);

    private void SkipVersion(string version)
    {
        _settings.SkippedUpdateVersion = version;
        _settingsService.Save(_settings);
    }

    private void OpenLink(object? parameter, bool isInstallLink)
    {
        if (parameter is not CliItemViewModel cliItem)
        {
            return;
        }

        string url = isInstallLink ? cliItem.Definition.InstallUrl : cliItem.Definition.DocsUrl;
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private string T(string key) => _localizationService.Translate(key, Language);

    private void NotifyLocalizedPropertiesChanged()
    {
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(TerminalLabel));
        OnPropertyChanged(nameof(RunAsAdministratorLabel));
        OnPropertyChanged(nameof(SupportedCliLabel));
        OnPropertyChanged(nameof(CliColumnIdLabel));
        OnPropertyChanged(nameof(CliColumnNameLabel));
        OnPropertyChanged(nameof(CliColumnCommandLabel));
        OnPropertyChanged(nameof(CliColumnStatusLabel));
        OnPropertyChanged(nameof(CliColumnEnabledLabel));
        OnPropertyChanged(nameof(CliColumnActionsLabel));
        OnPropertyChanged(nameof(LanguageSystemLabel));
        OnPropertyChanged(nameof(LanguageKoreanLabel));
        OnPropertyChanged(nameof(LanguageEnglishLabel));
        OnPropertyChanged(nameof(TerminalWindowsTerminalLabel));
        OnPropertyChanged(nameof(TerminalPowerShellLabel));
        OnPropertyChanged(nameof(RefreshLabel));
        OnPropertyChanged(nameof(RemoveAllLabel));
        OnPropertyChanged(nameof(ApplyLabel));
        OnPropertyChanged(nameof(CheckUpdateLabel));
        OnPropertyChanged(nameof(CurrentVersionLabel));
        OnPropertyChanged(nameof(UpdateCheckLabel));
        OnPropertyChanged(nameof(InstallLinkLabel));
        OnPropertyChanged(nameof(DocsLinkLabel));
        OnPropertyChanged(nameof(InstalledStatusLabel));
        OnPropertyChanged(nameof(NotInstalledStatusLabel));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
