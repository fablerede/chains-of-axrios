using System.Threading.Tasks;

public interface ICharacterDataService
{
    Task<CharacterData[]> LoadAllCharacters();
    Task SaveCharacter(CharacterData data);
    Task DeleteCharacter(string characterId);
}