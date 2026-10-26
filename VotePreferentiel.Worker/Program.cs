using VotePreferentiel;
using VotePreferentiel.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient<IVoteProvider, VoteProvider>();
builder.Services.AddSingleton<IVoteCounter, VoteCounter>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
