import { TokenService } from "./TokenService";
import decode from "jwt-decode";
import { LoginFailedException } from "../System/Exceptions/LoginFailedException";
import claimsTypes from "claimtypes";
import { Option as Maybe } from "./../System/Option_Maybe";

/**
 * Service to handle authentication
 * */
export class AuthenticationService {

    private readonly tokenService: TokenService;
    private readonly storage: Storage;
    private readonly TokenCookieName: string = "token";

    public constructor(public readonly url: string) {
        this.tokenService = new TokenService(url);
        this.storage = window.localStorage;
    }

    /**
     * Connects the specified account
     * 
     * @param {string} username 
     * @param {string} password
     */
    public async login(username: string, password: string): Promise<void> {
        let response = await this.tokenService.connect({ username, password });

        response.match(
            async (res) => {
                let token = await res;
                const decodedAccessToken: { expires: Date } = decode(token.accessToken);
                const decodedRefreshToken: { expires: Date } = decode(token.refreshToken);

                console.trace("Access token", decodedAccessToken);
                console.trace("Refresh token", decodedRefreshToken);

                this.storage.setItem(this.TokenCookieName, JSON.stringify(token));

            },
            async (errors) => {
                console.trace("Login failed", errors);
                throw new LoginFailedException(await errors);
            });
    }


    /**
     * Gets the token currently used
     * @returns {Maybe<MedEasy.DTO.BearerTokenInfo>} the token if any.
     */
    public getToken(): Maybe<MedEasy.DTO.BearerTokenInfo> {
        let optionalToken: Maybe<MedEasy.DTO.BearerTokenInfo> = Maybe.none();
        let tokenString = this.storage.getItem(this.TokenCookieName);
        if (tokenString && tokenString.trim().length > 0) {
            let token: MedEasy.DTO.BearerTokenInfo = JSON.parse(tokenString);

            optionalToken = Maybe.someWhenNotNull(token);
        }
        return optionalToken;
    }

    /**
     * Refresh access token by using the registered refresh token
     */
    public async renew(): Promise<void> {
        let optionalToken: Maybe<MedEasy.DTO.BearerTokenInfo> = this.getToken();

        optionalToken.matchSome(
            async (token) => {

                let accessToken = decode(token.accessToken);
                let isValid = new Date() < new Date(accessToken["exp"] * 1000);
                if (!isValid && this.isConnected()) {

                    let refreshToken = decode(token.refreshToken);
                    let optionalNewToken = await this.tokenService.refresh(refreshToken[claimsTypes.nameIdentifier], token);

                    optionalNewToken.matchSome(newToken => {
                        this.storage.removeItem(this.TokenCookieName);
                        this.storage.setItem(this.TokenCookieName, JSON.stringify(newToken));
                    });
                }
            }
        );
    }
    /**
     * Indicates wheter there is a valid refresh token in the system.
     * @returns {boolean} true if there's a valid refresh token
     * */
    public isConnected(): boolean {
        let optionalToken = this.getToken();

        return optionalToken.match<boolean>(
            (token) => {
                let refreshToken = token.refreshToken;
                let decodedRefreshToken = decode(refreshToken);

                return new Date(decodedRefreshToken["exp"] * 1000) > new Date();
            },
            () => false
        );
    }


    /**
     * Disconnects the user
     * 
     */
    public async logout(): Promise<void> {
        let optionalTokenString: Maybe<string> = Maybe.someWhenNotNull(this.storage.getItem(this.TokenCookieName));
        optionalTokenString.matchSome(async tokenString => {
            let token: MedEasy.DTO.BearerTokenInfo = JSON.parse(tokenString)
            let decodedAccessToken: { [key: string]: any } = decode(token.accessToken);
            await this.tokenService.delete(decodedAccessToken[claimsTypes.nameIdentifier], token);
            this.storage.removeItem(this.TokenCookieName);
        });
    }
}