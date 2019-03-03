import * as React from 'react';
import { History } from 'history';
import decode from "jwt-decode";

export interface BaseAuthenticatedComponentProps {
    history: History
}

/**
 * Base class for component that require authentication
 * */
export abstract class BaseAuthenticatedComponent<TProp extends BaseAuthenticatedComponentProps, TState> extends React.Component<TProp, TState>{

    private readonly storage: Storage;

    /**
     * Builds a new instance
     * @param props
     */
    public constructor(props: TProp) {
        super(props);
        this.storage = window.localStorage;
    }

    /**
     * Checks if there's a valid valid token 
     */
    protected isLoggedIn(): boolean {
        let tokenString: string = this.storage.getItem("token");

        let loggedIn: boolean = false;
        if (tokenString) {
            let token: MedEasy.DTO.BearerTokenInfo = JSON.parse(tokenString);
            if (token.refreshToken) {
                let decodedRefreshToken: { exp: number } = decode(token.refreshToken);
                loggedIn = Boolean(decodedRefreshToken.exp && new Date(decodedRefreshToken.exp * 1000) > new Date());
            }
        }

        return loggedIn;

    }

    public componentWillMount(): void {
        if (!this.isLoggedIn()) {
            this.props.history.replace("/sign-in");
        }
        
    }
}