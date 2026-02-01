import java.util.*;
public class Vehicle{
    String mark;
    String model;
    int year;
    public Vehicle(String mark, String model, int year){
        this.mark = mark;
        this.model = model;
        this.year = year;
    }
    public void StartEngine(){
        System.out.println("Engine started");
    }
    public void StopEngine(){
        System.out.println("Engine stopped");
    }
}
class Car extends Vehicle{
    int numberOfDoors;
    String transmissionType;
    public Car(String mark, String model, int year, int numberOfDoors, String transmissionType){
        super(mark, model, year);
        this.numberOfDoors = numberOfDoors;
        this.transmissionType = transmissionType;
    }
    @Override
    public void StartEngine(){
        System.out.println("Car engine started");
    }
    @Override
    public void StopEngine(){
        System.out.println("Car engine stopped");
    }
}
class Motorcycle extends Vehicle{
    String bodyType;
    String box;
    public Motorcycle(String mark, String model, int year, String bodyType, String box){
        super(mark, model, year);
        this.bodyType = bodyType;
        this.box = box;
    }
    @Override
    public void StartEngine(){
        System.out.println("Motorcycle engine started");
    }
    @Override
    public void StopEngine(){
        System.out.println("Motorcycle engine stopped");
    }
}
class Garage{
    List<Vehicle> vehicles;
    public Garage(){
        vehicles = new ArrayList<>();
    }
    public void addVehicle(Vehicle v){
        vehicles.add(v);
    }
    public void removeVehicle(Vehicle v){
        vehicles.remove(v);
    }
    public void listVehicles(){
        for(Vehicle v : vehicles){
            System.out.println(v.mark + " " + v.model + " " + v.year);
        }
    }
}
class Fleet{
    List<Garage> garages;
    public Fleet(){
        garages = new ArrayList<>();
    }
    public void addGarage(Garage g){
        garages.add(g);
    }
    public void removeGarage(Garage g){
        garages.remove(g);
    }
    public List<Vehicle> getAllVehicles(){
        List<Vehicle> allVehicles = new ArrayList<>();
        for(Garage g : garages){
            allVehicles.addAll(g.vehicles);
        }
        return allVehicles;
    }
}
class TestApp {
    public static void main(String[] args) {
        Garage g1 = new Garage();
        Garage g2 = new Garage();

        Vehicle car1 = new Car("Toyota", "Corolla", 2018, 4, "Automatic");
        Vehicle car2 = new Car("Ford", "Focus", 2015, 4, "Manual");
        Vehicle moto1 = new Motorcycle("Yamaha", "MT-07", 2020, "Naked", "None");

        g1.addVehicle(car1);
        g1.addVehicle(moto1);
        g2.addVehicle(car2);

        System.out.println("Гараж 1:");
        g1.listVehicles();

        System.out.println("Гараж 2:");
        g2.listVehicles();

        g1.removeVehicle(moto1);
        System.out.println("Гараж 1 после удаления мотоцикла:");
        g1.listVehicles();

        Fleet fleet = new Fleet();
        fleet.addGarage(g1);
        fleet.addGarage(g2);

        System.out.println("Все транспортные средства в автопарке:");
        for (Vehicle v : fleet.getAllVehicles()) {
            System.out.println(v.mark + " " + v.model + " " + v.year);
        }

        fleet.removeGarage(g2);
        System.out.println("Автопарк после удаления гаража 2:");
        for (Vehicle v : fleet.getAllVehicles()) {
            System.out.println(v.mark + " " + v.model + " " + v.year);
        }

        car1.StartEngine();
        car1.StopEngine();
    }
}