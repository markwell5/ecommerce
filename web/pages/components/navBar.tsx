import { AppBar, Toolbar, Typography, IconButton, Button, makeStyles, Theme, createStyles, Avatar } from "@material-ui/core";
import MenuIcon from "@material-ui/icons/Menu";
import AccountCircle from "@material-ui/icons/AccountCircle";
import { useAuth0 } from "@auth0/auth0-react";

const useStyles: any = makeStyles((theme: Theme) =>
    createStyles({
        title: {
            flexGrow: 1,
        },
    }),
);

export default function NavBar(): any {
    const classes: any = useStyles();
    const { loginWithRedirect, user, isAuthenticated, isLoading } = useAuth0();

    if (isLoading) {
        return null;
    }

    return (
        <AppBar>
            <Toolbar>
                <IconButton edge="start" color="inherit" aria-label="menu">
                    <MenuIcon />
                </IconButton>
                <Typography className={classes.title}>Ecommerce</Typography>
                <IconButton
                    aria-label="account of current user"
                    aria-controls="menu-appbar"
                    aria-haspopup="true"
                    onClick={() => loginWithRedirect()}
                    color="inherit"
                >
                    {isAuthenticated && <Avatar src={user.picture} />}
                    {!isAuthenticated && <AccountCircle />}
                </IconButton>
            </Toolbar>
        </AppBar>
    );
}