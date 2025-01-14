using Content.Shared.Language;
using Content.Shared.Language.Events;
using Content.Shared.Language.Systems;
using Robust.Shared.Console;

namespace Content.Client.Language.Systems;

/// <summary>
///   Client-side language system.
/// </summary>
/// <remarks>
///   Unlike the server, the client is not aware of other entities' languages; it's only notified about the entity that it posesses.
///   Due to that, this system stores such information in a static manner.
/// </remarks>
public sealed class LanguageSystem : SharedLanguageSystem
{
    /// <summary>
    ///   The current language of the entity currently possessed by the player.
    /// </summary>
    public string CurrentLanguage { get; private set; } = default!;
    /// <summary>
    ///   The list of languages the currently possessed entity can speak.
    /// </summary>
    public List<string> SpokenLanguages { get; private set; } = new();
    /// <summary>
    ///   The list of languages the currently possessed entity can understand.
    /// </summary>
    public List<string> UnderstoodLanguages { get; private set; } = new();

    [Dependency] private readonly IConsoleHost _consoleHost = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<LanguagesUpdatedMessage>(OnLanguagesUpdated);
    }

    /// <summary>
    ///   Sends a network request to the server to update this system's state.
    ///   The server may ignore the said request if the player is not possessing an entity.
    /// </summary>
    public void RequestStateUpdate()
    {
        RaiseNetworkEvent(new RequestLanguagesMessage());
    }

    public void RequestSetLanguage(LanguagePrototype language)
    {
        // May cause some minor desync...
        if (language.ID == CurrentLanguage)
            return;

        // (This is dumb. This is very dumb. It should be a message instead.)
        // TODO Change this, soonish
        _consoleHost.ExecuteCommand("languageselect " + language.ID);

        // So to reduce the probability of desync, we replicate the change locally too
        if (SpokenLanguages.Contains(language.ID))
            CurrentLanguage = language.ID;
    }

    private void OnLanguagesUpdated(LanguagesUpdatedMessage message)
    {
        CurrentLanguage = message.CurrentLanguage;
        SpokenLanguages = message.Spoken;
        UnderstoodLanguages = message.Understood;
    }
}
