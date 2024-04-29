 require 'dotkit'

task :publish => [:test] do
    if(!ENV['NUGET_KEY'].nil?)
        SECRETS['NUGET_KEY'] = ENV['NUGET_KEY']
        Dir.glob('**/*.nupkg'){|nupkg|
            if(nupkg.include?('Release'))
                PROJECT.run("dotnet nuget push #{nupkg} --skip-duplicate --api-key #{SECRETS['nuget_api_key']} --source https://api.nuget.org/v3/index.json")
            end
        }   
	else
		puts "NUGET_KEY not set"
    end
end



task :default => [:test,:integrate,:publish,:push]