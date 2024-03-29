# Fisco.dll - Biblioteca para Impressoras Térmicas

A `Fisco.dll` é uma biblioteca desenvolvida para facilitar o uso de impressoras térmicas em aplicações que requerem a geração de documentos fiscais, como cupons e recibos. A biblioteca oferece componentes para a renderização de textos, imagens e tabelas em um contexto gráfico, permitindo o controle preciso sobre o layout e o formato dos documentos a serem impressos.

<details>
  <summary>ℹ️ Limitações da Impressão Térmica</summary>
  
  Por ser uma imagem renderizada, a qualidade da impressão térmica pode variar. Fontes muito pequenas podem se tornar ilegíveis e a imagem pode parecer borrada, dependendo das configurações da impressora e da marca.
</details>

<details>
  <summary>✔️ Vantagens da Impressão Térmica</summary>
  
  - Impressão rápida e silenciosa
  - Baixo custo de manutenção, pois não requer tinta
  - Ideal para impressões de recibos, etiquetas e códigos de barras
  - Menor consumo de energia em comparação com impressoras tradicionais
</details>

<details>
  <summary>✔️ Vantagens da DLL de Impressão Térmica</summary>
  
  - Facilita a integração de impressão térmica em aplicativos C#
  - Abstrai complexidades de baixo nível, como configuração de impressora e formatação de imagem
  - Oferece suporte a recursos avançados, como alinhamento de elementos e criação de tabelas
  - Permite renderizar e imprimir imagens e texto com facilidade
</details>



## Componentes Principais

### `Image`

O componente `Image` permite a inserção e renderização de imagens em documentos fiscais. Ele oferece suporte para alinhamento e dimensionamento de imagens, garantindo que se ajustem corretamente ao layout do documento.

Exemplo de uso:
```csharp
var image = new Image(bitmap, ItemAlign.Center);
```

### `Text`

O componente `Text` facilita a adição de texto aos documentos fiscais. Ele oferece controle sobre a fonte, a cor e o alinhamento do texto, permitindo a criação de documentos com layout personalizado.

Exemplo de uso:
```csharp
var font = new Font("Arial", 12);
var text = new Text(font, "Exemplo de texto", ItemAlign.Left, Brushes.Black);
```

### `Table`

O componente `Table` permite a criação e renderização de tabelas em documentos fiscais. Ele oferece suporte para definição de colunas, linhas, estilos e alinhamentos, facilitando a organização e apresentação de dados tabulares.

Exemplo de uso:
```csharp
FiscoPapper fisco = new FiscoPapper(BobineSize._80x297mm, false)
var table = new Table(4, BobineSize._80x297mm)
{
    RowWrap = true,
    TableLineColor = Pens.Black
};

float[] widths = new float[] { 40, 20, 20, 20 };
table.SetPercentage(widths);

for (int i = 0; i < table.ColumnCount; i++)
{
    TableColumn column = new TableColumn($"COL {i + 1}")
    {
        DrawBackColor = true,
    };

    table.Columns.Add(column);
}

for (int i = 0; i < 40; i++)
{
    TableRow row = new TableRow(4);
    for (int j = 0; j < 4; j++)
        row.AddCell(new TableCell(new Text(new Font("Arial", 12f), $"({i}, {j})", ItemAlign.Left, i % 2 == 0 ? Brushes.Black : Brushes.White), i % 2 == 0 ? TableCell.BackColor.None : TableCell.BackColor.Black));

    table.Rows.Add(row);
}

fisco.AddComponent(table);
```

Exemplo de uso:
```csharp
try
{
    using (FiscoPapper fisco = new FiscoPapper(BobineSize._80x297mm, false))
    {
        Fisco.Component.Image myImage = new Fisco.Component.Image(new Bitmap(Image.FromFile("sourceFile"), new Size(250, 350)), ItemAlign.Center);
        fisco.AddComponent(myImage);

        img = fisco.Render();
    }
}
catch (Exception e)
{
    MessageBox.Show(e.Message);
}
```

Este código cria uma instância de `FiscoPapper` com o tamanho da bobina `80x297mm`, carrega uma imagem do arquivo `sourceFile`, redimensiona-a para `250x350` pixels, cria um objeto `Image` com a imagem e o alinhamento central e, em seguida, adiciona esse objeto ao `FiscoPapper`.

### Enum `ItemAlign`
| Valor       | Descrição                   |
|-------------|-----------------------------|
| `None`      | Indeterminado               |
| `Left`      | Alinhar à esquerda          |
| `Center`    | Alinhar no centro           |
| `Right`     | Alinhar à direita           |

**Descrição:** Alinhamento dos elementos gráficos.

### Enum `BobineSize`
| Valor           | Descrição                    |
|-----------------|------------------------------|
| `_58x297mm`     | 58mm X 297mm                |
| `_58x3276mm`    | 58mm X 3276mm               |
| `_80x297mm`     | 80mm X 297mm                |
| `_80x3276mm`    | 80mm X 3276mm               |

**Descrição:** Tipos de bobinas suportadas.

Essa tabela fornece uma visão detalhada dos valores e descrições associadas a cada enum na `Fisco.dll`.

## Funcionalidades Adicionais

Além dos componentes principais, a `Fisco.dll` também oferece funcionalidades para o gerenciamento de contexto gráfico e tratamento de exceções, garantindo uma experiência de uso robusta e confiável.

## Como Utilizar

1. Adicione a referência à `Fisco.dll` em seu projeto.
2. Utilize os componentes `Image`, `Text` e `Table` para criar o conteúdo do seu documento fiscal.
3. Renderize os componentes em um contexto gráfico, como uma `Graphics` do C#.
4. Envie o documento renderizado para a impressora térmica para impressão.

Com a `Fisco.dll`, você pode simplificar o processo de geração de documentos fiscais, tornando-o mais eficiente e preciso.
