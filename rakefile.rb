require 'dotkit'

task :setup do
  Raykit::DotNet::initialize_csharp_lib('Git.Files')
end

task :publish => [:test] do
    if(!ENV['NUGET_KEY'].nil?)
        SECRETS['NUGET_KEY'] = ENV['NUGET_KEY']
        Dir.glob('**/*.nupkg'){|nupkg|
            if(nupkg.include?('Release'))
                PROJECT.run("dotnet nuget push #{nupkg} --skip-duplicate --api-key #{SECRETS['NUGET_KEY']} --source https://api.nuget.org/v3/index.json")
            end
        }   
    end
end



task :default => [:test,:publish]