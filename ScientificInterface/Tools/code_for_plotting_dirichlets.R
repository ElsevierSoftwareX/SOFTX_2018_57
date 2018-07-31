#install.packages(" gtools")
library(gtools)

#Edit the names for the predators as necessary
Predator    <<- "Shark"
PreyNames   <<- c("Cod", "Haddock", "Whiting")
#Edit the proportions as they refer to each prey in the order named
means       <<- c(0.5,0.3,0.2)
#Specify which multiplier values you would like plots for
Multipliers <<- c(1,1.5,2,3,5,10,50,100)
#Specify the path to the location where you want the plots saved
save.path   <<- "C:/Users/Mark/Desktop/Dirichlet/"

pd = function(index){
  
  max.density=0
  
	#dev.new(width=10, height=3.5)

	for (iMultiplier in Multipliers){
	  a=hist(rdirichlet(1000000,means*iMultiplier)[,index], breaks=100, plot=FALSE)
	  max.density = max(max.density, a$density)
	  #cat ("Press [enter] to continue")
		#readline()
	  graphics.off()
	}
  
  for (iMultiplier in Multipliers){
    FILENAME = paste("Pred(",Predator,") Prey(",PreyNames[index],") Multiplier(",iMultiplier,")",sep='')
    png(filename = paste(save.path,FILENAME,".png",sep=""), res=900, width=10, height=4, units='in') 
    a=hist(rdirichlet(1000000,means*iMultiplier)[,index], breaks=100, main=paste("Prey",PreyNames[index],"\n\nm =", iMultiplier), xlab="The proportion the prey makes up of the predators diet", ylab="Probability density", xlim=c(0,1), freq=FALSE, ylim=c(0,max.density))
    graphics.off()
  }
}

for (iPrey in 1:length(means)){
  pd(iPrey)
}




