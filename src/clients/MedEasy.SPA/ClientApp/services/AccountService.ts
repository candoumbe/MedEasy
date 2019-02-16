import { SignUpFailedException } from "./../System/Exceptions/SignUpException";
import { LoginFailedException } from "./../System/Exceptions/LoginFailedException";
import { NewAccountInfo } from "./../components/accounts/NewAccountInfo";
import { AccountInfo } from "./../components/accounts/AccountInfo";
import { Browsable } from "./../restObjects/Browsable";
import { TokenService } from "./TokenService";

/**
 * Service to interact with account resources
 * */
export class AccountService {

    private readonly tokenService: TokenService;
    /**
     * Builds a new instance
     * @param endpoint url to endpoint that handle accounts
     * @param tokenUrl url to endpoint that handle token related stuff
     */
    public constructor(private readonly endpoint: string, tokenUrl : string) {
        this.tokenService = new TokenService(tokenUrl);
    }

    /**
     * Registers a new account
     * @param newAccount
     * @returns {MedEasy.DTO.BearerTokenInfo}
     */
    public async signUp(newAccount: NewAccountInfo): Promise<MedEasy.DTO.BearerTokenInfo> {
        let response: Response = await fetch(this.endpoint,
            {
                headers: { "Content-Type": "application/json" },
                method: "POST",
                body: JSON.stringify(newAccount)
            });

        if (!response.ok) {
            let errors = await response.json() as { [key: string]: Array<string> };
            throw new SignUpFailedException(errors);
        }

        let browsableAccount = await response.json() as Browsable<AccountInfo>;
        let optionalToken =  await this.tokenService.connect({
            username: newAccount.username, password: newAccount.password
        });

        return optionalToken.match<Promise<MedEasy.DTO.BearerTokenInfo>>(
            async (promise) => {
                let token = await promise;
                return token;
            },
            async (exception) => { throw new LoginFailedException(await exception); }
        );
    }

}