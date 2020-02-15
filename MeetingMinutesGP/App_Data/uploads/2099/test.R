data2<-read.csv("C:\\Users\\Hussein Ata\\Desktop\\bank_load.csv")
data2

DataLessThanFifty<-data2[data2$id<=50,]
DataLessThanFiftyAndGreaterThanZero<-DataLessThanFifty[DataLessThanFifty$id>0,]
DataLessThanHandred<-data2[data2$id<=100,]
DataLessThanHandredAndGreaterThanFifty<-DataLessThanHandred[DataLessThanHandred$id>50,]
DataLessThanTwoHandred<-data2[data2$id<=200,]
DataLessThanTwoHandredAndGreaterThanHandred<-DataLessThanTwoHandred[DataLessThanTwoHandred$id>100,]

plot(DataLessThanFiftyAndGreaterThanZero$id,DataLessThanFiftyAndGreaterThanZero$age)
lines(DataLessThanHandredAndGreaterThanFifty$age,lty=1)
x1<rep(1:5,times=5)
x1