import { Option } from "./../System/Option_Either";

interface ErrorInfo { [key: string]: Array<string> }
export class TokenService {

    /**
     * Builds a new instance
     * @param url the endpoint where to ask for new token
     */
    public constructor(readonly url: string) {

    }

    /**
     * Requests a new token
     * @param data 
     */
    public async connect(data: { username: string, password: string }): Promise<Option<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<{ [key: string]: Array<string> }>>> {
        let response = await fetch(this.url, {
            headers: { "content-type": "application/json" },
            method: "POST",
            body: JSON.stringify(data)
        });

        let optionalResult: Option<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<{ [key: string]: Array<string> }>>;

        if (response.ok) {
            optionalResult = Option.some<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<{ [key: string]: Array<string> }>>(await response.json() as Promise<MedEasy.DTO.BearerTokenInfo>)
        } else {
            optionalResult = Option.none<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<{ [key: string]: Array<string> }>>(await response.json() as Promise<{ [key: string]: Array<string> }>)
        }

        return optionalResult;
    }

    /**
     * Requests a new token
     * @param data 
     */
    public async refresh(username: string, token: MedEasy.DTO.BearerTokenInfo): Promise<Option<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<{ [key: string]: string | Array<string> }>>> {
        let response = await fetch(`${this.url}/${username}`, {
            method: "PATCH",
            headers: { "Authorization": `Bearer ${token.accessToken}` },
            body: JSON.stringify(token)
        });

        let optionalResult: Option<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<ErrorInfo>>;

        if (response.ok) {
            optionalResult = Option.some<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<ErrorInfo>>(await response.json() as Promise<MedEasy.DTO.BearerTokenInfo>)
        } else {
            optionalResult = Option.none<Promise<MedEasy.DTO.BearerTokenInfo>, Promise<ErrorInfo>>(await response.json() as Promise<ErrorInfo>)
        }

        return optionalResult;
    }

    /**
     * Invalidate the user's access token
     * @param username
     * @param token
     */
    public async delete(username: string, token: MedEasy.DTO.BearerTokenInfo): Promise<void> {
        await fetch(
            `${this.url}/${username}`,
            {
                headers: { "Authorization": `Bearer ${token.accessToken}` },
                method: "DELETE",
                body: JSON.stringify(token)
            });
    }
}