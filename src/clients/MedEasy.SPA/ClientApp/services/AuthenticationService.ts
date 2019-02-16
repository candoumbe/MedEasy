import { TokenService } from "./TokenService";
import decode from "jwt-decode";
import { LoginFailedException } from "../System/Exceptions/LoginFailedException";
import claimsTypes from "claimtypes";
import { Option as Maybe} from "./../System/Option_Maybe";

/**
 * Service to handle authentication
 * */
export class AuthenticationService {

    private readonly tokenService: TokenService;
    private readonly storage: Storage;
    private readonly TokenCookieName: string = "token";

    public constructor(readonly url: string) {
        this.tokenService = new TokenService(url);
        this.storage = window.localStorage;
    }

    /**
     * Connects the user
     * 
     * @param username
     * @param password
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


    public getToken(): Maybe<MedEasy.DTO.BearerTokenInfo> {
        let optionalToken: Maybe<MedEasy.DTO.BearerTokenInfo>;
        let tokenString = this.storage.getItem(this.TokenCookieName);
        if (tokenString && tokenString.trim().length > 0) {
            let token: MedEasy.DTO.BearerTokenInfo = JSON.parse(tokenString);

            optionalToken = Maybe.someWhenNotNull(token);
        }
        return optionalToken;
    }

    /**
     * Disconnects the user
     * 
     */
    public async logout(): Promise<void> {
        let tokenString: string = this.storage.getItem(this.TokenCookieName);
        let token: MedEasy.DTO.BearerTokenInfo = JSON.parse(tokenString)
        if (token) {
            let decodedAccessToken: {[key: string]: any} = decode(token.accessToken);
            await this.tokenService.delete(decodedAccessToken[claimsTypes.nameIdentifier], token);
            this.storage.removeItem(this.TokenCookieName);
        }
    }
}