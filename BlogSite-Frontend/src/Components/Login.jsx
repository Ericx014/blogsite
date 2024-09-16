import {useNavigate, Link} from "react-router-dom";
import {useContext, useEffect} from "react";
import {BlogContext} from "../App";
import AuthServices from "../services/login";
import UserServices from "../services/users";
import LoginForm from "./LoginForm";
import Notification from "./Notification";

const Login = () => {
  const {
    username,
    password,
    setUsername,
    setPassword,
    setToken,
    notification,
    setNotification,
    notificationType,
    setNotificationType,
		setCurrentUser
  } = useContext(BlogContext);
  const navigate = useNavigate();

  useEffect(() => {
    if (notification) {
      const timeout = setTimeout(() => {
        setNotification("");
        setNotificationType("");
      }, 3000);
      return () => clearTimeout(timeout);
    }
  }, [notification, notificationType, setNotification, setNotificationType]);

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const responseData = await AuthServices.login(username, password);
      loginSuccess(responseData, username);
    } catch (error) {
      loginFail(error);
    }
  };
  const loginSuccess = async (responseData, loggedInUsername) => {
    localStorage.setItem("blogsiteToken", JSON.stringify(responseData.token));
		setToken(responseData.token);
    
		setPassword("");
		const loggedInUser = await UserServices.getUserByUsername(loggedInUsername);
		console.log(loggedInUser);
		
		localStorage.setItem("blogUser", JSON.stringify(loggedInUser));
		setCurrentUser(loggedInUser);
    
		navigate("/blogs");
  };
  const loginFail = (error) => {
    setNotification("Wrong username or password");
    setNotificationType("error");
    
		console.error("Login failed:", error);
  };

	const registrationLink = (
    <>
      <p>
        Dont have an account?{" "}
        <Link to="/register">
          <button className="font-semibold hover:underline transition-all">
            Create one.
          </button>
        </Link>
      </p>
    </>
  );

  return (
    <section>
      <Notification />
      <h1 className="font-bold uppercase tracking-wider text-xl mb-4">
        Log in
      </h1>
      <LoginForm
        handleLogin={handleLogin}
        username={username}
        setUsername={setUsername}
        password={password}
        setPassword={setPassword}
      />
      {registrationLink}
    </section>
  );
};

export default Login;
