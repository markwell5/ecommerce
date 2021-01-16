import styles from "../styles/Home.module.css";
import NavBar from "./components/navBar";

export default function Home(): any {
  return (
    <div className={styles.container}>
      <NavBar></NavBar>
    </div>
  );
}
