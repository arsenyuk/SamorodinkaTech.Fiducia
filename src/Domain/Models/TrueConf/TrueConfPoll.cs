namespace SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

/// <summary>Опрос (poll) в конференции TrueConf.</summary>
public class TrueConfPoll
{
    /// <summary>Идентификатор опроса (id).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Текст вопроса (question).</summary>
    public string Question { get; init; } = string.Empty;

    /// <summary>Варианты ответов (options).</summary>
    public List<TrueConfPollOption> Options { get; init; } = new();

    /// <summary>Состояние опроса: active, closed (state).</summary>
    public string State { get; init; } = string.Empty;

    /// <summary>Результаты голосования (results).</summary>
    public TrueConfPollResults? Results { get; init; }
}

/// <summary>Вариант ответа в опросе TrueConf.</summary>
public class TrueConfPollOption
{
    /// <summary>Идентификатор варианта (id).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Текст варианта (text).</summary>
    public string Text { get; init; } = string.Empty;
}

/// <summary>Результаты голосования в опросе TrueConf.</summary>
public class TrueConfPollResults
{
    /// <summary>Общее число проголосовавших (total_voters).</summary>
    public int TotalVoters { get; init; }

    /// <summary>Голоса по вариантам (votes).</summary>
    public List<TrueConfPollVoteCount> Votes { get; init; } = new();
}

/// <summary>Количество голосов за вариант.</summary>
public class TrueConfPollVoteCount
{
    /// <summary>Идентификатор варианта (option_id).</summary>
    public string OptionId { get; init; } = string.Empty;

    /// <summary>Количество голосов (count).</summary>
    public int Count { get; init; }
}

/// <summary>Запрос на создание опроса в TrueConf.</summary>
public class CreateTrueConfPollRequest
{
    /// <summary>Текст вопроса (question).</summary>
    public string Question { get; init; } = string.Empty;

    /// <summary>Варианты ответов (options).</summary>
    public List<TrueConfPollOption> Options { get; init; } = new();

    /// <summary>Анонимное голосование (anonymous).</summary>
    public bool Anonymous { get; init; }

    /// <summary>Показывать результаты до голосования (show_results_before_vote).</summary>
    public bool ShowResultsBeforeVote { get; init; }
}

/// <summary>Информация о сервере TrueConf (product).</summary>
public class TrueConfServerInfo
{
    /// <summary>Отображаемое имя сервера (display_name).</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Идентификатор сервера (id).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Название продукта (name).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Версия сервера (version).</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Платформа (platform).</summary>
    public string Platform { get; init; } = string.Empty;

    /// <summary>URL документации (links.documentation).</summary>
    public string DocumentationUrl { get; init; } = string.Empty;

    /// <summary>URL сайта TrueConf (links.site_url).</summary>
    public string SiteUrl { get; init; } = string.Empty;

    /// <summary>URL веб-конфигурации (web_config.url).</summary>
    public string WebConfigUrl { get; init; } = string.Empty;
}
