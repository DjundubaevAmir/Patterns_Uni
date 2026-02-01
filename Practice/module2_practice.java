import java.util.ArrayList;
import java.util.List;

public class module2_practice {
	public static void main(String[] args) {
		UserManager manager = new UserManager();

		manager.addUser(new User("Amir", "amir@example.com", "Admin"));
		manager.addUser(new User("Mikhail", "mikhail@example.com", "User"));

		manager.printAll();

		manager.updateUser("mikhail@example.com", new User("Mikhail", "mikhail@example.com", "Manager"));
		manager.removeUser("amir@example.com");

		manager.printAll();
	}
}

class User {
	private String name;
	private String email;
	private String role;

	public User(String name, String email, String role) {
		this.name = name;
		this.email = email;
		this.role = role;
	}

	public String getName() { return name; }
	public void setName(String name) { this.name = name; }

	public String getEmail() { return email; }
	public void setEmail(String email) { this.email = email; }

	public String getRole() { return role; }
	public void setRole(String role) { this.role = role; }

	@Override
	public String toString() {
		return String.format("User{name='%s', email='%s', role='%s'}", name, email, role);
	}
}
class UserManager {
	private final List<User> users = new ArrayList<>();

	public void addUser(User user) {
		int idx = findIndexByEmail(user.getEmail());
		if (idx == -1) {
			users.add(user);
		} else {
			users.set(idx, user);
		}
	}

	public void removeUser(String email) {
		int idx = findIndexByEmail(email);
		if (idx != -1) {
			users.remove(idx);
		}
	}

	public void updateUser(String email, User updatedUser) {
		int idx = findIndexByEmail(email);
		if (idx != -1) {
			updatedUser.setEmail(users.get(idx).getEmail());
			users.set(idx, updatedUser);
		}
	}

	public List<User> getAllUsers() {
		return new ArrayList<>(users);
	}

	public void printAll() {
		System.out.println("Users:");
		for (User u : users) {
			System.out.println("  " + u);
		}
	}

	private int findIndexByEmail(String email) {
		for (int i = 0; i < users.size(); i++) {
			if (users.get(i).getEmail().equals(email)) {
				return i;
			}
		}
		return -1;
	}
}