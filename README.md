# Instagram Follower Tracker

## Sobre
O **Instagram Follower Tracker** é uma ferramenta para monitorar a lista de seguidores de uma conta do Instagram.  
Ele realiza um login automatizado (com suporte a 2FA, se necessário), extrai o ID do usuário e coleta a lista de seguidores usando chamadas à API gráfica do Instagram.  
Além disso, o programa permite salvar a lista de seguidores em um arquivo JSON e fazer comparações entre diferentes extrações para identificar alterações.

## Funcionalidades
- **Login Automatizado:** Realiza login utilizando o campo `enc_password` no formato exigido pelo Instagram.
- **Suporte para 2FA:** Caso a autenticação de dois fatores esteja habilitada, o programa solicita o código e efetua o login.
- **Extração de Seguidores:** Obtém o ID do usuário e extrai a lista de seguidores via requisições à API.
- **Salvar & Comparar Dados:** Permite salvar a lista de seguidores em um arquivo JSON datado e comparar duas listas para identificar mudanças.

## Requisitos
- [.NET SDK](https://dotnet.microsoft.com/download) instalado na máquina.
- Conexão com a internet.
- Conta válida no Instagram para autenticação.

## Instalação
1. Faça o clone deste repositório:

    ```bash
    git clone https://github.com/seu-usuario/InstagramFollowerTracker.git
    ```

2. Navegue até o diretório do projeto:

    ```bash
    cd InstagramFollowerTracker
    ```

3. Restaure os pacotes NuGet:

    ```bash
    dotnet restore
    ```

## Configuração
O projeto utiliza a biblioteca [Newtonsoft.Json](https://www.newtonsoft.com/json) para manipulação de JSON.  
Certifique-se de que todos os pacotes estejam corretamente restaurados ao executar `dotnet restore`.

## Uso
Para executar o projeto, utilize o comando:

```bash
dotnet run
```

Ao iniciar, o programa irá solicitar:

- **Usuário do Instagram:** Digite seu nome de usuário.
- **Senha do Instagram:** Digite sua senha (a senha é transformada em um valor criptografado no formato exigido pelo Instagram).

Caso a autenticação de dois fatores (2FA) esteja ativa na conta, o sistema pedirá o código de verificação.

**Exemplo de execução:**

```bash
Instagram Follower Tracker
Enter your Instagram username: seu_usuario
Enter your Instagram password: sua_senha
Status Code: OK
Login realizado com sucesso!
```

Ao final do processo, a lista de seguidores é obtida e salva em um arquivo JSON, com nome no formato `followers_YYYY-MM-DD.json`.

## Estrutura do Projeto
- **InstagramScraper.cs:**  
  Contém a implementação principal com os métodos para:
    - Obter o token CSRF e configurar os headers da requisição.
    - Realizar o login e, se necessário, a autenticação de dois fatores.
    - Extrair o ID do usuário e coletar a lista de seguidores.
    - Salvar e comparar listas de seguidores.

- **Outros Arquivos:**  
  Podem conter configurações adicionais, scripts ou testes de unidade conforme a evolução do projeto.

## Considerações Importantes
- **Manutenção:**  
  O Instagram pode alterar seus endpoints e forma de autenticação a qualquer momento, o que pode afetar a funcionalidade deste scraper. Mantenha o projeto atualizado e verifique periodicamente se a estrutura das respostas mudou.

- **Uso Responsável:**  
  Certifique-se de utilizar esta ferramenta de forma ética e em conformidade com os Termos de Serviço do Instagram. O autor não se responsabiliza por usos indevidos.

## Contribuições
Contribuições são bem-vindas! Se você quiser melhorar este projeto, sinta-se à vontade para:
- Abrir issues para reportar bugs ou sugerir melhorias.
- Submeter pull requests com correções e novas funcionalidades.

## Licença
Este projeto está licenciado sob a licença MIT. Consulte o arquivo `LICENSE` para mais informações.

## Aviso Legal
Este software é fornecido "no estado em que se encontra", sem garantias de qualquer tipo, explícitas ou implícitas.  
Use-o por sua conta e risco. O autor não se responsabiliza por eventuais problemas decorrentes do uso desta ferramenta.