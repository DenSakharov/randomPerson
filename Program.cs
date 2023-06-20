using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Security.Principal;

class ListPerson
{
    public List<Person> results { get; set; }
}
class Person
{
    public Int32 Id { get; set; }
    public Guid TransportId { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public Int32 SequenceId { get; set; }
    public String[] CreditCardNumbers { get; set; }
    public Int32 Age { get; set; }
    public String[] Phones { get; set; }
    public Int64 BirthDate { get; set; }
    public Double Salary { get; set; }
    public System.Boolean IsMarred { get; set; }
    public Gender Gender { get; set; }
    public Child[] Children { get; set; }
}
class Child
{
    public Int32 Id { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public Int64 BirthDate { get; set; }
    public Gender Gender { get; set; }
}
enum Gender
{
    Male,
    Female
}

//для апи случаниого ФИО
public class Result
{
    public List<Results> results { get; set; }
}
public class Results
{
    public Name name { get; set; }
    public Dob dob { get; set; }
    public string phone { get; set; }
    public string cell { get; set; }
}
public class Name
{
    public string title { get; set; }
    
    public string first { get; set; }
    public string last { get; set; }
}
public class Dob
{
    public DateTime date { get; set; }

    public int age { get; set; }
}


class Program
{
    private static string APIUrl = "https://randomuser.me/api/?inc=name,dob,phone,cell&noinfo";
    public static string GetDataWithoutAuthentication()
    {
        using (var client = new WebClient())
        {
            client.Headers.Add("Content-Type:application/json");
            //client.Headers.Add("Accept:application/json");

            var result = client.DownloadString(APIUrl);
            //Console.WriteLine(Environment.NewLine + result);
            return result;
        }
    }
    static void Main(string[] args)
    {
        List<Person> list = new List<Person>();
        for(int i = 0; i < 10000
            ; i++)
        {
            Person person = new Person();
            person.Id = i;
            Guid g = Guid.NewGuid();
            person.TransportId =g;
            string resultAPI = GetDataWithoutAuthentication();
            Result res = JsonConvert.DeserializeObject<Result>(resultAPI);
           
            person.FirstName = res.results[0].name.first;
            person.LastName = res.results[0].name.last;
            person.SequenceId =i;

            Random rnd = new Random();

            Random rnd1 = new Random();
            List<string> strForCards=new List<string>();   
            for (int j=0;j< rnd1.Next(1,10);j++) 
            {
                long numberCard = (rnd.Next(int.MaxValue - rnd.Next(int.MaxValue / 2), int.MaxValue) + rnd.Next(int.MaxValue - rnd.Next(int.MaxValue / 2), int.MaxValue));
                strForCards.Add( numberCard.ToString()); 
            }
            person.CreditCardNumbers = strForCards.ToArray(); 

            person.Age = res.results[0].dob.age;

            strForCards.Clear();
            strForCards.Add(res.results[0].phone);
            strForCards.Add(res.results[0].cell);
            person.Phones = strForCards.ToArray();
            string test = res.results[0].dob.date.Date.ToShortDateString().Replace(".","");
            person.BirthDate = Convert.ToInt64(test);

            person.Salary =rnd.Next(100000,1000000);

            person.IsMarred =( 2 == rnd.Next(1, 2)) ? true: false;
            person.Gender = (1 == rnd.Next(1, 2)) ? Gender.Male : Gender.Female;
            
            List<Child> childList = new List<Child>();
            for(int k = 0; k < rnd1.Next(1, 11); k++)
            {
                Child ch = new Child();
                ch.Id= k;
                ch.Gender= (1 == rnd.Next(1, 2) )? Gender.Male : Gender.Female;
                ch.FirstName = (ch.Gender== Gender.Male)?"Ivan":"Sasha";
                ch.LastName=person.LastName;
                ch.BirthDate = person.BirthDate+18+k;
                childList.Add(ch);
            }
            person.Children =childList.ToArray();

            list.Add(person);
        }
        //Console.WriteLine(list.Count);
        ListPerson personList = new ListPerson();
        personList.results = new List<Person>();
        personList.results.AddRange(list);
        var json = System.Text.Json.JsonSerializer.Serialize(personList);
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        File.WriteAllText(path+@"\person.json", json);
        personList.results.Clear();

        var resultt = JsonConvert.DeserializeObject<ListPerson>(File.ReadAllText(path + @"\person.json"));

        int sumCard = 0;

        int dateNow=Convert.ToInt32( DateTime.Now.Year);
        int sumChildrenCount = 0;
        int sumChildrenAge = 0;
        foreach (Person person in resultt.results)
        {
            //Console.WriteLine(person.FirstName + " ; " + person.LastName + " \n");
            sumCard += person.CreditCardNumbers.Count();
            int averageAge = 0;
            foreach(var child in person.Children)
            {
                averageAge += dateNow - Convert.ToInt32( child.BirthDate%10000);//dateNow-Convert.ToInt32(child.BirthDate);
            }
            averageAge = averageAge / person.Children.Count();
            sumChildrenAge += averageAge;
            sumChildrenCount += person.Children.Count();
        }
        sumChildrenAge = sumChildrenAge / sumChildrenCount;

        Console.WriteLine("persons count -> " + resultt.results.Count);
        Console.WriteLine("persons credit card count -> " + sumCard);
        Console.WriteLine("the average value of child age. -> " + sumChildrenAge);
    }
}